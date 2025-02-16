namespace YGOHandAnalysisFramework.Data.Formatting;

public sealed class PercentFormat<T> : IFormat<T>
{
    public static IFormat<T> Default { get; } = new PercentFormat<T>();

    public string FormatData(T value)
    {
        return $"{value:P2}";
    }
}
