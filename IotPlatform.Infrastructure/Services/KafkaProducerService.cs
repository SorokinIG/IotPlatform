using Confluent.Kafka;
using IotPlatform.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IotPlatform.Infrastructure.Services
{
    // Infrastructure/Services/KafkaProducerService.cs
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<Null, string> _producer;
        private readonly string _topic;

        public KafkaProducerService(IConfiguration config)
        {
            var bootstrapServers = config["Kafka:BootstrapServers"];
            _topic = config["Kafka:Topic"];

            _producer = new ProducerBuilder<Null, string>(new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            }).Build();
        }

        public async Task ProduceAsync(string message)
        {
            await _producer.ProduceAsync(_topic, new Message<Null, string>
            {
                Value = message
            });
        }
    }
}
