using ESAM.GrowTracking.Application.ValueObjects;

namespace ESAM.GrowTracking.Application.Abstractions.Results
{
    public interface IResult
    {
        bool IsSuccess { get; }
        IReadOnlyList<Error> Errors { get; }
    }

    public interface IResult<T> : IResult
    {
        T Value { get; }
    }
}