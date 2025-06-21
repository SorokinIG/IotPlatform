
using IotPlatform.Core.Entities;

namespace IotPlatform.Core.Interfaces
{
    public interface IDeviceRepository : IRepository<Device>
    {
        Task<IEnumerable<Device>> GetByTypeAsync(string type);
        Task<bool> ExistsAsync(Guid deviceId);
    }
}
