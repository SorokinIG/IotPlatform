
using IotPlatform.Core.Entities;
using IotPlatform.Core.Interfaces;
using IotPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Infrastructure.Repositories
{
    public class DeviceRepository : BaseRepository<Device>, IDeviceRepository
    {
        public DeviceRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Device>> GetByTypeAsync(string type)
            => await _context.Devices.Where(d => d.Type == type).ToListAsync();

        public async Task<bool> ExistsAsync(Guid deviceId)
            => await _context.Devices.AnyAsync(d => d.Id == deviceId);
    }
}
