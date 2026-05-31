namespace ESAM.GrowTracking.Application.Enums
{
    public enum ErrorType : byte
    {
        Validation = 1,
        NotFound = 2,
        Unauthorized = 3,
        Forbidden = 4,
        Conflict = 5,
        BusinessRule = 6,
        UnprocessableEntity = 7,
        Locked = 8,
        ServerError = 9
    }
}