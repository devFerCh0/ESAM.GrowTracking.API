namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IClientInfoService
    {
        string? GetIpAddress();

        string? GetUserAgent();
    }
}