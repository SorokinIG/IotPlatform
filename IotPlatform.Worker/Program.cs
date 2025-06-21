using IotPlatform.Core.Interfaces;
using IotPlatform.Infrastructure.Data;
using IotPlatform.Infrastructure.Repositories;
using IotPlatform.Infrastructure.Services;
using IotPlatform.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

// Регистрируем репозитории
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

// Kafka Consumer
builder.Services.AddHostedService<KafkaTelemetryConsumer>();

var host = builder.Build();
host.Run();
