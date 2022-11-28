using Constants;
using DemoMicroservices.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// create the logger and setup your sinks, filters and properties
Log.Logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .CreateLogger();

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderHasBeenCreatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(RabbitMqConsts.ROOT, "/", h => {
            h.Username(RabbitMqConsts.USERNAME);
            h.Password(RabbitMqConsts.PASSWORD);
        });
       
        cfg.ReceiveEndpoint("api-gateway--order-has-been-created", re =>
        {
            re.PrefetchCount = 16;
            re.UseMessageRetry(r => r.Interval(2, 100));
            re.ConfigureConsumer<OrderHasBeenCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(a =>
{
    a.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo Microservices");
    a.InjectStylesheet("/swagger/custom.css");
    a.RoutePrefix = "";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

