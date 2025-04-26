using System;

namespace Tubes;

// Based on Zoran Horvat's video at https://www.youtube.com/watch?v=LXF-rRWaIxc with modifications.
public sealed class Result<T, TError>
{
    private readonly T? _value;
    private readonly TError? _error;
    
    public bool IsSuccess { get; }

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Result is not successful");

    public TError Error => !IsSuccess ? _error! : throw new InvalidOperationException("Result is successful");

    private Result(bool isSuccess, T? value, TError? error) => 
        (IsSuccess, _value, _error) = (isSuccess, value, error);
    
    public static Result<T, TError> Success(T value) => new(true, value, default);
    public static Result<T, TError> Failure(TError error) => new (false, default, error);
}