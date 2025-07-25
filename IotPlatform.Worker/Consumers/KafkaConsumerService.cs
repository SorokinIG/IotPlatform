using Confluent.Kafka;
using IotPlatform.Core.Entities;
using IotPlatform.Core.Interfaces;
using System.Text.Json;

namespace IotPlatform.Infrastructure.Services
{
    // Worker/Consumers/KafkaTelemetryConsumer.cs
    public class KafkaTelemetryConsumer : BackgroundService
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly IServiceProvider _services;
        private readonly ILogger<KafkaTelemetryConsumer> _logger;

        public KafkaTelemetryConsumer(
            IConfiguration config,
            IServiceProvider services,
            ILogger<KafkaTelemetryConsumer> logger)
        {
            _services = services;
            _logger = logger;

            var bootstrapServers = config["Kafka:BootstrapServers"];
            var groupId = config["Kafka:GroupId"];

            _consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "iot-worker-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("telemetry");
            _logger.LogInformation("Consumer started for topic: telemetry");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = _consumer.Consume(stoppingToken);
                    using var scope = _services.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ITelemetryRepository>();

                    var data = JsonSerializer.Deserialize<TelemetryData>(message.Message.Value);
                    _logger.LogInformation($"Deserialized: DeviceId={data.DeviceId}, Value={data.Value}");
                    await repo.AddAsync(data);
                    await repo.SaveAsync();
                    _logger.LogInformation("Data saved to DB");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки сообщения");
                }
            }
        }
    }
}
