using FluentValidation;
using IotPlatform.Core;
using IotPlatform.Core.Interfaces;
using IotPlatform.Core.Validators;
using IotPlatform.Infrastructure.Data;
using IotPlatform.Infrastructure.Repositories;
using IotPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

/* 
 План поэтапной разработки IoT платформы (IoT платформа для обработки событий с устройств)
1. Проектирование архитектуры
Цель: Определить основные компоненты системы и технологии.

Стек:

Бэкенд: ASP.NET Core 8 (Web API, gRPC)

Базы данных: PostgreSQL (основная БД), Redis (кэш), TimescaleDB (для временных рядов)

Брокер сообщений: Kafka / RabbitMQ

Контейнеризация: Docker (+ Docker Compose для локального развертывания)

Мониторинг: Prometheus + Grafana

Схема:

Устройства → (HTTP/gRPC) → API Gateway → (Kafka) → Обработчик событий → (PostgreSQL/TimescaleDB)  
                             ↘ (Redis) → Кэш  
                             ↘ (Prometheus) → Мониторинг  
2. Настройка базового API (REST + gRPC)
Цель: Создать эндпоинты для приема данных с устройств.

Реализовать:

POST /api/devices/{id}/telemetry – прием телеметрии

GET /api/devices/{id}/stats – получение статистики

gRPC сервис для быстрой передачи бинарных данных

3. Работа с брокером сообщений (Kafka/RabbitMQ)
Цель: Обеспечить асинхронную обработку событий.

Настроить Producer (отправка данных в Kafka)

Настроить Consumer (обработка данных и сохранение в БД)

4. Хранение и анализ данных
PostgreSQL – устройства, пользователи, метаданные

TimescaleDB (расширение PostgreSQL) – временные ряды (температура, влажность и т. д.)

Redis – кэширование частых запросов

5. Мониторинг и логирование

Мониторинг (Prometheus + Grafana)

Метрики запросов к API

Графики нагрузки Redis

Serilog + Elasticsearch – логирование

6.Авторизация (JWT + RSA)

Роли: Устройство/Пользователь/Админ

Шифрование чувствительных данных

7.Stream-соединения
Эффективной передачи частых данных с устройств.

Управления устройствами в реальном времени.

Визуализации телеметрии без опроса (polling).

Оптимизации нагрузки (агрегация перед записью в БД).

авторизация для gRPC и SignalR 

8. Оптимизация и масштабирование
Добавить Rate Limiting (ограничение запросов)

Настроить Kubernetes (если будем деплоить в облако)
 */
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = "redis:6379";
    options.InstanceName = "IoTPlatform_";
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IoT Platform API",
        Version = "v1"
    });
});
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", builder => {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<DeviceDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<TelemetryDataDtoValidator>();

//DI
builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

builder.WebHost.ConfigureKestrel(options => {
    options.ListenAnyIP(80);

    options.ListenAnyIP(443, listenOptions => {
        var certPath = "/https/iotplatformapi.pfx";
        Console.WriteLine($"Пытаемся загрузить сертификат из: {certPath}");

        if (!File.Exists(certPath))
        {
            throw new FileNotFoundException($"Файл сертификата не найден: {certPath}");
        }

        try
        {
            var cert = new X509Certificate2(
                certPath,
                "YourPassword123",
                X509KeyStorageFlags.EphemeralKeySet |
                X509KeyStorageFlags.Exportable);

            listenOptions.UseHttps(cert);
            Console.WriteLine("Сертификат успешно загружен");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки сертификата: {ex}");
            throw;
        }
    });
});



var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IoT Platform API v1");
    });
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();


app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();
