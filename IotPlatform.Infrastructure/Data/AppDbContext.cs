using IotPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace IotPlatform.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<TelemetryData> TelemetryData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Здесь можно добавить конфигурацию моделей (опционально)
            base.OnModelCreating(modelBuilder);

            
        }
    }
}