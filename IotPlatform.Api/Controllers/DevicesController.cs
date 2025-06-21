using AutoMapper;
using FluentValidation;
using IotPlatform.Core.DTOs;
using IotPlatform.Core.Entities;
using IotPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace IotPlatform.Api.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceRepository _repo;
        private readonly IMapper _mapper;
        private readonly IValidator<DeviceDto> _validator;
        public DevicesController(IValidator<DeviceDto> validator, 
            IDeviceRepository repo,
            IMapper mapper) 
        {
            _repo = repo;
            _mapper = mapper;
            _validator = validator;
        } 

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetAll()
        {
            var devices = await _repo.GetAllAsync();
            return Ok(_mapper.Map<IEnumerable<DeviceDto>>(devices));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var device = await _repo.GetByIdAsync(id);
            return device == null ? NotFound() : Ok(device);
        }

        

        [HttpPost]
        public async Task<ActionResult<DeviceDto>> Create(DeviceDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                // 2. Возврат кастомных ошибок
                var errors = validationResult.ToDictionary();
                return BadRequest(new { Errors = errors });
            }

            var device = _mapper.Map<Device>(dto);
            await _repo.AddAsync(device);
            await _repo.SaveAsync();
            return CreatedAtAction(nameof(GetById), new { id = device.Id },
                   _mapper.Map<DeviceDto>(device));
        }
    }
}
