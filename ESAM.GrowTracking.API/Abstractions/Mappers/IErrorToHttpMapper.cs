using ESAM.GrowTracking.Application.Enums;

namespace ESAM.GrowTracking.API.Abstractions.Mappers
{
    public interface IErrorToHttpMapper
    {
        int GetStatusCode(IEnumerable<ErrorType> errorTypes);
    }
}