
using System.ComponentModel.DataAnnotations;

namespace IotPlatform.Core.DTOs
{
    public class TelemetryDataDto
    {
        [Required]
        public Guid DeviceId { get; set; }

        [Range(-100, 100)]
        public double Value { get; set; }

        [StringLength(50)]
        public string MetricType { get; set; }
    }
}
