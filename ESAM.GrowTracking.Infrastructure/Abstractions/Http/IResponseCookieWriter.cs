using ESAM.GrowTracking.Infrastructure.Http;

namespace ESAM.GrowTracking.Infrastructure.Abstractions.Http
{
    public interface IResponseCookieWriter
    {
        void Append(string name, string value, ResponseCookieDescriptor options);

        void Delete(string name, ResponseCookieDescriptor options);
    }
}