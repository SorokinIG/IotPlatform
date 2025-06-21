using FluentValidation;
using IotPlatform.Core;
using IotPlatform.Core.Interfaces;
using IotPlatform.Core.Validators;
using IotPlatform.Infrastructure.Data;
using IotPlatform.Infrastructure.Repositories;
using IotPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

/* 
 План поэтапной разработки IoT платформы
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
Prometheus – сбор метрик (количество запросов, задержки)

Grafana – визуализация

Serilog + Elasticsearch – логирование

6. Оптимизация и масштабирование
Добавить Rate Limiting (ограничение запросов)

Настроить Kubernetes (если будем деплоить в облако)
 */
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<DeviceDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<TelemetryDataDtoValidator>();

//DI
builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<ITelemetryRepository, TelemetryRepository>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
