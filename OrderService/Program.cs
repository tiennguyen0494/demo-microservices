using Constants;
using MassTransit;
using OrderService.Entities;
using OrderService.Consumers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddMvc().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = "localhost";
    option.InstanceName = "demo-redis";
});

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultString"]);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CreateUserConsumer>();
    x.AddConsumer<CreateOrderConsumer>();
    x.AddConsumer<ProductsAreReadyForSaleConsumer>();
    x.AddConsumer<ProductsAreNotReadyForSaleConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(RabbitMqConsts.ROOT, "/", h =>
        {
            h.Username(RabbitMqConsts.USERNAME);
            h.Password(RabbitMqConsts.PASSWORD);
        });

        cfg.ReceiveEndpoint("order--create-user-order-model", re =>
        {
            //re.PrefetchCount = 16;
            //re.UseMessageRetry(r => r.Interval(2, 100));
            re.ConfigureConsumer<CreateUserConsumer>(context);
        });

        cfg.ReceiveEndpoint("order--create-order", re =>
        {
            //re.PrefetchCount = 16;
            //re.UseMessageRetry(r => r.Interval(2, 100));
            re.ConfigureConsumer<CreateOrderConsumer>(context);
        });

        cfg.ReceiveEndpoint("order--products-are-not-ready-for-sale", re =>
        {
            //re.PrefetchCount = 16;
            //re.UseMessageRetry(r => r.Interval(2, 100));
            re.ConfigureConsumer<ProductsAreNotReadyForSaleConsumer>(context);
        });

        cfg.ReceiveEndpoint("order--products-are-ready-for-sale", re =>
        {
            //re.PrefetchCount = 16;
            //re.UseMessageRetry(r => r.Interval(2, 100));
            re.ConfigureConsumer<ProductsAreReadyForSaleConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(a =>
{
    a.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service");
    a.InjectStylesheet("/swagger/custom.css");
    a.RoutePrefix = "";
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

