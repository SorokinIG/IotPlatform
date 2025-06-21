using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotPlatform.Core.Entities
{
    public class Device
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // "sensor", "actuator"
        public string Location { get; set; }
    }
}
