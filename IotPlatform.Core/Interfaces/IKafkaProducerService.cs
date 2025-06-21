using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotPlatform.Core.Interfaces
{
    public interface IKafkaProducerService
    {
        Task ProduceAsync(string message);
    }
}
