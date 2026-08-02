namespace Application.Common.Results;

public abstract class Result<T>
{
    public bool IsSuccess { get; protected init; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; protected init; }
    public T? Value { get; protected init; }

    public static Result<T> Success(T value) => new SuccessResult(value);
    public static Result<T> Failure(string error) => new FailureResult(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }

    private class SuccessResult : Result<T>
    {
        public SuccessResult(T value)
        {
            Value = value;
            IsSuccess = true;
        }
    }

    private class FailureResult : Result<T>
    {
        public FailureResult(string error)
        {
            Error = error;
            IsSuccess = false;
        }
    }
}

public static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}
