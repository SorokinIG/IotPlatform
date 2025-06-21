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

            _consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                BootstrapServers = config["Kafka:BootstrapServers"],
                GroupId = "iot-platform-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("telemetry");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = _consumer.Consume(stoppingToken);
                    using var scope = _services.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ITelemetryRepository>();

                    var data = JsonSerializer.Deserialize<TelemetryData>(message.Message.Value);
                    await repo.AddAsync(data);
                    await repo.SaveAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки сообщения");
                }
            }
        }
    }
}
