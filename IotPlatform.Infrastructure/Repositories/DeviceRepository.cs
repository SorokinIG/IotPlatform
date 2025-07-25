
using IotPlatform.Core.Entities;
using IotPlatform.Core.Interfaces;
using IotPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace IotPlatform.Infrastructure.Repositories
{
    public class DeviceRepository : BaseRepository<Device>, IDeviceRepository
    {
        private readonly IDistributedCache _cache;

        public DeviceRepository(AppDbContext context, 
            IDistributedCache cache) : base(context) 
        { 
            _cache = cache;
        }

        public async Task<IEnumerable<Device>> GetByTypeAsync(string type)
            => await _context.Devices.Where(d => d.Type == type).ToListAsync();

        public async Task<bool> ExistsAsync(Guid deviceId)
            => await _context.Devices.AnyAsync(d => d.Id == deviceId);

        public async Task<Device> GetDeviceCachedAsync(Guid deviceId)
        {
            var cacheKey = $"device_{deviceId}";

            // Пытаемся получить данные из Redis
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                return JsonSerializer.Deserialize<Device>(cachedData);
            }

            // Если нет в кэше, загружаем из БД
            var device = await _context.Devices.FindAsync(deviceId);
            if (device != null)
            {
                // Сериализуем и сохраняем в Redis
                var serializedDevice = JsonSerializer.Serialize(device);
                await _cache.SetStringAsync(
                    cacheKey,
                    serializedDevice,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    });
            }

            return device;
        }
    }
}
