using Constants;
using MassTransit;
using UserService.Entities;
using Microsoft.EntityFrameworkCore;
using UserService.Consumers;

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

builder.Services.AddDbContext<UserDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultString"]);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CreateUserConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(RabbitMqConsts.ROOT, "/", h =>
        {
            h.Username(RabbitMqConsts.USERNAME);
            h.Password(RabbitMqConsts.PASSWORD);
        });

        cfg.ReceiveEndpoint("user--create-an-user", re =>
        {
            re.PrefetchCount = 16;
            re.UseMessageRetry(r => r.Interval(2, 100));
            re.ConfigureConsumer<CreateUserConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(a =>
{
    a.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service");
    a.InjectStylesheet("/swagger/custom.css");
    a.RoutePrefix = "";
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

