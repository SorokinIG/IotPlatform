using FluentValidation;
using IotPlatform.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotPlatform.Core.Validators
{
    public class TelemetryDataDtoValidator : AbstractValidator<TelemetryDataDto>
    {
        public TelemetryDataDtoValidator()
        {
            RuleFor(x => x.DeviceId).NotEmpty();
            RuleFor(x => x.Value).InclusiveBetween(-100, 100);
            RuleFor(x => x.MetricType).MaximumLength(50);
        }
    }
}
