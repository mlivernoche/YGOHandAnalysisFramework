using System.Collections.Immutable;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

public interface IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    string Name { get; }
    IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName> GetResults(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers);
}
