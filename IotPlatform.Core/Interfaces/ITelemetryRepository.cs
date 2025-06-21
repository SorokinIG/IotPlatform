
using IotPlatform.Core.Entities;

namespace IotPlatform.Core.Interfaces
{
    public interface ITelemetryRepository : IRepository<TelemetryData>
    {
        Task<IEnumerable<TelemetryData>> GetByDeviceIdAsync(Guid deviceId);
        Task<IEnumerable<TelemetryData>> GetLatestAsync(int count);
        Task<PagedResponse<TelemetryData>> GetPagedAsync(Guid deviceId, int page, int pageSize);
    }
}
