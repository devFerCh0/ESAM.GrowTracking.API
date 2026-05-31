namespace ESAM.GrowTracking.Infrastructure.Abstractions.Validators
{
    public interface ICorsOriginValidator
    {
        bool IsOriginAllowed(string origin);
    }
}