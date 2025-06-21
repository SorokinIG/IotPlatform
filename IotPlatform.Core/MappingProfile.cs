
using AutoMapper;
using IotPlatform.Core.DTOs;
using IotPlatform.Core.Entities;

namespace IotPlatform.Core
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Device, DeviceDto>();
            CreateMap<DeviceDto, Device>();
            CreateMap<TelemetryData, TelemetryDataDto>();
            CreateMap<TelemetryDataDto, TelemetryData>();
        }
    }
}
