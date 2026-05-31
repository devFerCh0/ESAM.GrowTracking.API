namespace ESAM.GrowTracking.Infrastructure.Abstractions.Http
{
    public interface IHttpRequestContextReader
    {
        string? GetCookieValue(string cookieName);

        string? GetHeaderValue(string headerName);

        string? GetRemoteIpAddress();

        string? GetUserAgent();
    }
}