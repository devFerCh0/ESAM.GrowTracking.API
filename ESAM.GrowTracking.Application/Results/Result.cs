using ESAM.GrowTracking.Application.Abstractions.Results;
using ESAM.GrowTracking.Application.ValueObjects;

namespace ESAM.GrowTracking.Application.Results
{
    public class Result : IResult
    {
        private readonly List<Error> _errors;

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public IReadOnlyList<Error> Errors => _errors;

        protected Result(bool isSuccess, IEnumerable<Error>? errors)
        {
            var errorList = errors?.ToList() ?? [];
            if (isSuccess && errorList.Count != 0)
                throw new InvalidOperationException("Un resultado exitoso no puede contener errores.");
            if (!isSuccess && errorList.Count == 0)
                throw new InvalidOperationException("Un resultado fallido debe contener al menos un error.");
            IsSuccess = isSuccess;
            _errors = errorList;
        }

        public static Result Ok() => new(true, null);

        public static Result Fail(Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return new Result(false, [error]);
        }

        public static Result Fail(IEnumerable<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);
            return new Result(false, errors);
        }

        public static Result Combine(params Result[] results)
        {
            ArgumentNullException.ThrowIfNull(results);
            var failedErrors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToList();
            return failedErrors.Count != 0 ? Fail(failedErrors) : Ok();
        }
    }

    public sealed class Result<T> : Result, IResult<T>
    {
        public T Value { get; }

        private Result(bool isSuccess, T? value, IEnumerable<Error>? errors) : base(isSuccess, errors)
        {
            if (isSuccess)
            {
                ArgumentNullException.ThrowIfNull(value);
                Value = value!;
            }
            else
                Value = default!;
        }

        public static Result<T> Ok(T value) => new(true, value, null);

        public new static Result<T> Fail(Error error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return new Result<T>(false, default, [error]);
        }

        public new static Result<T> Fail(IEnumerable<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);
            return new Result<T>(false, default, errors);
        }

        public Result AsResult() => IsSuccess ? Ok() : Result.Fail(Errors);
    }
}