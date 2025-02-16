using System.Numerics;

namespace YGOHandAnalysisFramework.Data.Formatting;

public sealed class CardinalFormat<T> : IFormat<T> where T : INumber<T>
{
    public static IFormat<T> Default { get; } = new CardinalFormat<T>();

    public string FormatData(T value)
    {
        return $"{value:N3}";
    }
}
