using YGOHandAnalysisFramework.Data.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public static class DataComparisonCategoryResultsRanked
{
    public static IDataComparisonCategoryResults Create<TComparison, TReturn>(string name, IFormat<TReturn> format, IReadOnlyDictionary<TComparison, TReturn> results, TimeSpan executionTime, IComparer<TReturn> comparer)
        where TComparison : notnull, IDataComparisonFormatterEntry
    {
        var dict = results.ToDictionary<KeyValuePair<TComparison, TReturn>, IDataComparisonFormatterEntry, TReturn>(static kv => kv.Key, kv => kv.Value);
        return new DataComparisonCategoryResultsRanked<TReturn>(name, format, dict, executionTime, comparer);
    }
}

public sealed class DataComparisonCategoryResultsRanked<TReturn> : IDataComparisonCategoryResults
{
    private Dictionary<IDataComparisonFormatterEntry, TReturn> Results { get; }
    private IFormat<TReturn> Format { get; }
    private IComparer<TReturn> Comparer { get; }
    public string Name { get; }
    public TimeSpan ExecutionTime { get; }

    public DataComparisonCategoryResultsRanked(string name, IFormat<TReturn> format, IReadOnlyDictionary<IDataComparisonFormatterEntry, TReturn> results, TimeSpan executionTime, IComparer<TReturn> comparer)
    {
        Name = name;
        Format = format;
        Results = new Dictionary<IDataComparisonFormatterEntry, TReturn>(results);
        ExecutionTime = executionTime;
        Comparer = comparer;
    }

    public string GetResult(IDataComparisonFormatterEntry key)
    {
        if (Results.TryGetValue(key, out var result))
        {
            var place = 1;

            {
                foreach (var otherResult in Results.OrderBy(static kv => kv.Value, Comparer))
                {
                    if (otherResult.Key.Equals(key))
                    {
                        break;
                    }

                    place++;
                }
            }

            return $"{Format.FormatData(result)} ({place:N0})";
        }

        return string.Empty;
    }
}
