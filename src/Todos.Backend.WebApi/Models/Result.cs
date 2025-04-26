namespace Todos.Backend.WebApi.Models;

/// <summary>
/// Represents the result of an operation with success or failure
/// </summary>
/// <typeparam name="TSuccess">The type of the success value</typeparam>
public class Result<TSuccess>
{
    private readonly TSuccess? _value;
    private readonly string? _error;
    private readonly bool _isSuccess;

    private Result(TSuccess value)
    {
        _value = value;
        _isSuccess = true;
        _error = null;
    }

    private Result(string error)
    {
        _error = error;
        _isSuccess = false;
        _value = default;
    }

    public static Result<TSuccess> Success(TSuccess value) => new Result<TSuccess>(value);
    public static Result<TSuccess> Failure(string error) => new Result<TSuccess>(error);

    public TResult Match<TResult>(
        Func<TSuccess, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return _isSuccess ? onSuccess(_value!) : onFailure(_error!);
    }
} 