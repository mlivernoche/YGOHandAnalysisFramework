using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public sealed class HandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName, TReturn> : IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> Results { get; }
    private IFormat<TReturn> Format { get; }
    public string Name { get; }
    public TimeSpan ExecutionTime { get; }

    public HandAnalyzerComparisonCategoryResults(string name, IFormat<TReturn> format, IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> results, TimeSpan executionTime)
    {
        Name = name;
        Format = format;
        Results = new Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn>(results);
        ExecutionTime = executionTime;
    }

    public string GetResult(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
    {
        if (Results.TryGetValue(handAnalyzer, out var result))
        {
            return $"{Format.FormatData(result)}";
        }

        return string.Empty;
    }
}
