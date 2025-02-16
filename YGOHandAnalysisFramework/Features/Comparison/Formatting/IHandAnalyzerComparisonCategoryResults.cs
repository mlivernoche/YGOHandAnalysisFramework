using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;

namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public interface IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    string Name { get; }
    TimeSpan ExecutionTime { get; }
    string GetResult(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer);
}
