using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Configuration;

internal sealed class DefaultConfiguration<TCardGroupName>(IHandAnalyzerOutputStream outputStream, CreateDataComparisonFormatter comparisonFormatter) : IConfiguration<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public IEnumerable<IConfigurationDeckList<TCardGroupName>> DeckLists { get; } = [];
    public IHandAnalyzerOutputStream OutputStream { get; } = outputStream;
    public CreateDataComparisonFormatter FormatterFactory { get; } = comparisonFormatter;
    public int CardListFillSize { get; } = 40;
    public IEnumerable<int> HandSizes { get; } = [5, 6];
    public bool CreateWeightedProbabilities { get; }
    public bool UseCache { get; }
    public string CacheLocation => string.Empty;
}
