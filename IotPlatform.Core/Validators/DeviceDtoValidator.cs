using FluentValidation;
using IotPlatform.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotPlatform.Core.Validators
{
    public class DeviceDtoValidator : AbstractValidator<DeviceDto>
    {
        public DeviceDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Имя устройства обязательно")
                .MaximumLength(100).WithMessage("Максимум 100 символов");

            RuleFor(x => x.Type)
                .Must(BeValidType).WithMessage("Допустимые типы: Sensor, Actuator");
        }

        private bool BeValidType(string type)
            => new[] { "Sensor", "Actuator" }.Contains(type);
    }
}
