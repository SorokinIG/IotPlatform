# IoT Platform

![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15.0-blue)
![Docker](https://img.shields.io/badge/Docker-Ready-blue)
![Kafka](https://img.shields.io/badge/Apache-Kafka-orange)
![Redis](https://img.shields.io/badge/Redis-Caching-red)

Платформа для обработки событий с IoT устройств с поддержкой реального времени и масштабируемой архитектурой.

## 🏗️ Архитектура

### Компоненты системы
- **IotPlatform.Api** - основной Web API для приема телеметрии
- **IotPlatform.Worker** - фоновый сервис для обработки сообщений Kafka
- **IotPlatform.Core** - доменная логика и модели данных
- **IotPlatform.Infrastructure** - доступ к данным и внешние сервисы
- **IotPlatform.Tests** - модульные тесты

### Технологический стек
- **Backend**: ASP.NET Core 8, Docker
- **Database**: PostgreSQL, Redis (кэширование)
- **Message Broker**: Apache Kafka
- **Validation**: FluentValidation
- **Documentation**: Swagger/OpenAPI
- **Mapping**: AutoMapper

## 🚀 Быстрый старт

### Предварительные требования
- Docker & Docker Compose
- .NET 8 SDK
- PostgreSQL 15+

### Локальная разработка

```bash
# Клонирование репозитория
git clone <your-repo-url>
cd iot-platform

# Запуск инфраструктуры
docker-compose up -d

# Запуск приложения
dotnet run --project IotPlatform.Api
```

### Docker развертывание

```bash
# Сборка и запуск всех сервисов
docker-compose up -d --build
```

После запуска приложение будет доступно:
- API: https://localhost:32769
- PostgreSQL: localhost:5432
- Redis: localhost:6379
- Kafka: localhost:9092

## 📡 API Endpoints

### Устройства

#### Получить все устройства
```http
GET /api/devices
```

#### Получить устройство по ID
```http
GET /api/devices/{id}
```

#### Создать устройство
```http
POST /api/devices
Content-Type: application/json

{
  "name": "Temperature Sensor",
  "type": "sensor",
  "location": "Room 101"
}
```

### Телеметрия

#### Отправить данные телеметрии
```http
POST /api/telemetry
Content-Type: application/json

{
  "deviceId": "guid",
  "value": 23.5,
  "metricType": "temperature"
}
```

#### Получить телеметрию устройства
```http
GET /api/telemetry/{deviceId}
```

#### Получить постраничную телеметрию
```http
GET /api/telemetry/{deviceId}/paged?page=1&pageSize=20
```

## 💾 Модели данных

### Device
```csharp
public class Device
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } // "sensor", "actuator"
    public string Location { get; set; }
}
```

### TelemetryData
```csharp
public class TelemetryData
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string MetricType { get; set; } // "temperature", "humidity"
}
```

## 🔧 Конфигурация

### appsettings.json
```json
{
  "Kafka": {
    "BootstrapServers": "kafka:9092",
    "Topic": "telemetry",
    "GroupId": "iot-worker-group"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=iot_db;Username=postgres;Password=postgres"
  }
}
```

### Docker Compose Services
- **api**: ASP.NET Core API (порты 8080, 32769)
- **db**: PostgreSQL с health checks
- **kafka**: Apache Kafka с автоматическим созданием топиков
- **redis**: Кэширование Redis
- **worker**: Фоновый обработчик сообщений

## 🏭 Обработка сообщений

### Поток данных
```
Устройство → HTTP POST → API → Kafka → Worker → PostgreSQL
                    ↘ Redis (кэш)
```

### Kafka Integration
- **Producer**: Отправка телеметрии в топик `telemetry`
- **Consumer**: Обработка сообщений воркером и сохранение в БД

## 🔒 Безопасность

- HTTPS с пользовательскими сертификатами
- CORS политика "AllowAll" для разработки
- Валидация данных с FluentValidation

## 🧪 Тестирование

```bash
# Запуск тестов
dotnet test

# Запуск с покрытием
dotnet test --collect:"XPlat Code Coverage"
```

## 📁 Структура проекта

```
IotPlatform/
├── IotPlatform.Api/
│   ├── Controllers/          # API контроллеры
│   ├── Middleware/           # Пользовательская middleware
│   ├── Program.cs            # Конфигурация приложения
│   └── Dockerfile           # Контейнеризация API
├── IotPlatform.Core/
│   ├── Entities/            # Доменные модели
│   ├── DTOs/               # Data Transfer Objects
│   ├── Interfaces/          # Контракты репозиториев
│   └── Validators/          # FluentValidation правила
├── IotPlatform.Infrastructure/
│   ├── Data/               # DbContext и миграции
│   ├── Repositories/       # Реализации репозиториев
│   └── Services/           # Внешние сервисы (Kafka)
├── IotPlatform.Worker/
│   ├── Consumers/          # Kafka consumers
│   └── Dockerfile          # Контейнеризация Worker
└── docker-compose.yml      # Оркестрация контейнеров
```

## 🚢 Деплоймент

### Production сборка
```bash
docker-compose -f docker-compose.yml build --no-cache
```

### Миграции базы данных
Миграции выполняются автоматически при запуске приложения через `dbContext.Database.Migrate()`.
