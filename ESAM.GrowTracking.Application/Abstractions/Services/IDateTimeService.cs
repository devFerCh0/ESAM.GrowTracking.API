namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IDateTimeService
    {
        DateTime UtcNow { get; }
    }
}