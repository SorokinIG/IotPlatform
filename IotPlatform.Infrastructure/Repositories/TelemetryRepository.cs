
using IotPlatform.Core.Entities;
using IotPlatform.Core.Interfaces;
using IotPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Infrastructure.Repositories
{
    public class TelemetryRepository : BaseRepository<TelemetryData>, ITelemetryRepository
    {
        public TelemetryRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<TelemetryData>> GetByDeviceIdAsync(Guid deviceId)
            => await _context.TelemetryData
                .Where(t => t.DeviceId == deviceId)
                .OrderByDescending(t => t.Timestamp)
                .ToListAsync();

        public async Task<IEnumerable<TelemetryData>> GetLatestAsync(int count)
            => await _context.TelemetryData
                .OrderByDescending(t => t.Timestamp)
                .Take(count)
                .ToListAsync();

        public async Task<PagedResponse<TelemetryData>> GetPagedAsync(Guid deviceId, int page, int pageSize)
        {
            var query = _context.TelemetryData
                .Where(t => t.DeviceId == deviceId)
                .OrderByDescending(t => t.Timestamp);

            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<TelemetryData>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Data = data
            };
        }
    }
}
