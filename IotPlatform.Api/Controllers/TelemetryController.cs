using AutoMapper;
using FluentValidation;
using IotPlatform.Core.DTOs;
using IotPlatform.Core.Entities;
using IotPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;

namespace IotPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/telemetry")]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryRepository _telemetryRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly IMapper _mapper;
        private IValidator<TelemetryDataDto> _validator;
        private readonly IKafkaProducerService _kafkaProducer;

        public TelemetryController(
            IValidator<TelemetryDataDto> validator,
            ITelemetryRepository telemetryRepo,
            IDeviceRepository deviceRepo,
            IMapper mapper,
            IKafkaProducerService kafkaProducer)
        {
            _telemetryRepo = telemetryRepo;
            _deviceRepo = deviceRepo;
            _mapper = mapper;
            _validator = validator;
            _kafkaProducer = kafkaProducer;
        }

        [HttpPost]
        public async Task<IActionResult> AddTelemetry([FromBody] TelemetryDataDto dto)
        {
            // 1. Валидация
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Errors = validationResult.ToDictionary() });
            }

            // 2. Проверка устройства
            if (!await _deviceRepo.ExistsAsync(dto.DeviceId))
                return BadRequest("Device not found");

            await _kafkaProducer.ProduceAsync(JsonSerializer.Serialize(new
            {
                dto.DeviceId,
                dto.Value,
                dto.MetricType,
                Timestamp = DateTime.UtcNow
            }));

            return Accepted(); // 202 Accepted
        }

        [HttpGet("{deviceId}")]
        public async Task<ActionResult<IEnumerable<TelemetryDataDto>>> GetTelemetry(Guid deviceId)
        {
            var data = await _telemetryRepo.GetByDeviceIdAsync(deviceId);
            return Ok(_mapper.Map<IEnumerable<TelemetryDataDto>>(data));
        }

        [HttpGet("{deviceId}/paged")]
        public async Task<ActionResult<PagedResponse<TelemetryDataDto>>> GetPaged(
        Guid deviceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("Номер страницы и размер должны быть больше 0");

            var response = await _telemetryRepo.GetPagedAsync(deviceId, page, pageSize);
            return Ok(new PagedResponse<TelemetryDataDto>
            {
                Page = response.Page,
                PageSize = response.PageSize,
                TotalCount = response.TotalCount,
                Data = _mapper.Map<List<TelemetryDataDto>>(response.Data)
            });
        }
    }
}
