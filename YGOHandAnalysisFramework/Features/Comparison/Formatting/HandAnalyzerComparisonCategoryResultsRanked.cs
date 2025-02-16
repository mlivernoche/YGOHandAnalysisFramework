using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public sealed class HandAnalyzerComparisonCategoryResultsRanked<TCardGroup, TCardGroupName, TReturn> : IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> Results { get; }
    private IFormat<TReturn> Format { get; }
    private IComparer<TReturn> Comparer { get; }
    public string Name { get; }
    public TimeSpan ExecutionTime { get; }

    public HandAnalyzerComparisonCategoryResultsRanked(string name, IFormat<TReturn> format, IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> results, TimeSpan executionTime, IComparer<TReturn> comparer)
    {
        Name = name;
        Format = format;
        Results = new Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn>(results);
        ExecutionTime = executionTime;
        Comparer = comparer;
    }

    public string GetResult(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
    {
        if (Results.TryGetValue(handAnalyzer, out var result))
        {
            var place = 1;

            {
                foreach (var otherResult in Results.OrderBy(static kv => kv.Value, Comparer))
                {
                    if (otherResult.Key == handAnalyzer)
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
