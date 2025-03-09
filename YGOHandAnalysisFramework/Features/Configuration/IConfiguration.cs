using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Configuration;

public interface IConfiguration<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    IEnumerable<IConfigurationDeckList<TCardGroupName>> DeckLists { get; }
    IHandAnalyzerOutputStream OutputStream { get; }
    CreateDataComparisonFormatter FormatterFactory { get; }
    int CardListFillSize { get; }
    IEnumerable<int> HandSizes { get; }
    bool CreateWeightedProbabilities { get; }
    bool UseCache { get; }
}
