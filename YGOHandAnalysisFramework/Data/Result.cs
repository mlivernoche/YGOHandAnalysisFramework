using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace YGOHandAnalysisFramework.Data;

public readonly struct Result<T, E> : IEquatable<Result<T, E>>
{
    private readonly bool Success { get; }
    private readonly T? Value { get; }
    private readonly E? Error { get; }

    public Result(T value)
    {
        Guard.IsNotNull(value);
        Success = true;
        Value = value;
    }

    public Result(E error)
    {
        Guard.IsNotNull(error);
        Success = false;
        Error = error;
    }

    public readonly bool GetResult([NotNullWhen(true)] out T? result, [NotNullWhen(false)] out E? error)
    {
        result = default;
        error = default;
        if (Success)
        {
            Guard.IsNotNull(Value);
            result = Value;
        }
        else
        {
            Guard.IsNotNull(Error);
            error = Error;
        }

        return Success;
    }

    public readonly bool Equals(Result<T, E> other)
    {
        return
            Success == other.Success &&
            Equals(Value, other.Value) &&
            Equals(Error, other.Error);
    }

    public readonly override bool Equals(object? obj)
    {
        return obj is Result<T, E> other && Equals(other);
    }
    public static bool operator ==(Result<T, E> left, Result<T, E> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Result<T, E> left, Result<T, E> right)
    {
        return !(left == right);
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(Success, Value, Error);
    }
}
