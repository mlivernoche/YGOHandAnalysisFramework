using System.Diagnostics;

namespace YGOHandAnalysisFramework.Features.Combinations;

[DebuggerDisplay("{HandName}: {MinimumSize} // {MaximumSize}")]
public readonly struct HandElement<TCardGroupName> : IEquatable<HandElement<TCardGroupName>>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public required TCardGroupName HandName { get; init; }
    public required int MinimumSize { get; init; }
    public required int MaximumSize { get; init; }

    public HandElement<TCardGroupName> Add(HandElement<TCardGroupName> other)
    {
        if (!HandName.Equals(other.HandName))
        {
            return this;
        }

        var min = MinimumSize + other.MinimumSize;
        var max = Math.Max(MaximumSize, other.MaximumSize);
        max = Math.Max(min, max);

        return new HandElement<TCardGroupName>()
        {
            HandName = HandName,
            MinimumSize = min,
            MaximumSize = max,
        };
    }

    public bool Equals(HandElement<TCardGroupName> other)
    {
        return
            HandName.Equals(other.HandName) &&
            MinimumSize == other.MinimumSize &&
            MaximumSize == other.MaximumSize;
    }

    public override bool Equals(object? obj)
    {
        return obj is HandElement<TCardGroupName> other && Equals(other);
    }

    public static bool operator ==(HandElement<TCardGroupName> left, HandElement<TCardGroupName> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HandElement<TCardGroupName> left, HandElement<TCardGroupName> right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(HandName, MinimumSize, MaximumSize);
    }
}
