using ESAM.GrowTracking.Application.Abstractions.Services;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public sealed class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}