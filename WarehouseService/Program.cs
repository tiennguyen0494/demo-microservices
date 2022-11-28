using Constants;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserService.Entities;
using WarehouseService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = "localhost";
    option.InstanceName = "demo-redis";
});

builder.Services.AddDbContext<WarehouseDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultString"]);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<InventoryConsumer>();
    x.AddConsumer<OrderHasBeenCreatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(RabbitMqConsts.ROOT, "/", h => {
            h.Username(RabbitMqConsts.USERNAME);
            h.Password(RabbitMqConsts.PASSWORD);
        });

        cfg.ReceiveEndpoint("warehouse--check-products-in-warehouse", re =>
        {
            re.PrefetchCount = 16;
            re.UseMessageRetry(r => r.Interval(2, 100));
            re.ConfigureConsumer<InventoryConsumer>(context);
        });

        cfg.ReceiveEndpoint("warehouse--order-has-been-created", re =>
        {
            re.PrefetchCount = 16;
            re.UseMessageRetry(r => r.Interval(2, 100));
            re.ConfigureConsumer<OrderHasBeenCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(a =>
{
    a.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse Service");
    a.InjectStylesheet("/swagger/custom.css");
    a.RoutePrefix = "";
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

