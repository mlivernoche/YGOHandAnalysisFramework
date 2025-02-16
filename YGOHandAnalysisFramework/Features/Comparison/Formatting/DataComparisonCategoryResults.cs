using YGOHandAnalysisFramework.Data.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public static class DataComparisonCategoryResults
{
    public static IDataComparisonCategoryResults Create<TComparison, TReturn>(string name, IFormat<TReturn> format, IReadOnlyDictionary<TComparison, TReturn> results, TimeSpan executionTime)
        where TComparison : notnull, IDataComparisonFormatterEntry
    {
        var dict = results.ToDictionary<KeyValuePair<TComparison, TReturn>, IDataComparisonFormatterEntry, TReturn>(static kv => kv.Key, kv => kv.Value);
        return new DataComparisonCategoryResults<TReturn>(name, format, dict, executionTime);
    }
}

public sealed class DataComparisonCategoryResults<TReturn> : IDataComparisonCategoryResults
{
    private Dictionary<IDataComparisonFormatterEntry, TReturn> Results { get; }
    private IFormat<TReturn> Format { get; }
    public string Name { get; }
    public TimeSpan ExecutionTime { get; }

    public DataComparisonCategoryResults(string name, IFormat<TReturn> format, IReadOnlyDictionary<IDataComparisonFormatterEntry, TReturn> results, TimeSpan executionTime)
    {
        Name = name;
        Format = format;
        Results = new(results);
        ExecutionTime = executionTime;
    }

    public string GetResult(IDataComparisonFormatterEntry key)
    {
        if (Results.TryGetValue(key, out var result))
        {
            return $"{Format.FormatData(result)}";
        }

        return string.Empty;
    }
}
