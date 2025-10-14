using System.Diagnostics.CodeAnalysis;

namespace Trivare.Application.DTOs.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    [AllowNull]
    public T Value { get; }
    public ErrorResponse? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
    }

    private Result(ErrorResponse error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(ErrorResponse error) => new(error);
}
