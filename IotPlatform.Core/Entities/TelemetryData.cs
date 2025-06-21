using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotPlatform.Core.Entities
{
    public class TelemetryData
    {
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; } // температура, влажность и т. д.
        public string MetricType { get; set; } // "temperature", "humidity"
    }
}
