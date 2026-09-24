namespace SkillSwap.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string ErrorMessage { get; }
    public ErrorType ErrorType { get; }

    private Result(bool isSuccess, T? data, string errorMessage, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
    }

    public static Result<T> Success(T data) => new(true, data, string.Empty, ErrorType.None);

    public static Result<T> Failure(ErrorType type, string message) => new(false, default, message, type);
}
