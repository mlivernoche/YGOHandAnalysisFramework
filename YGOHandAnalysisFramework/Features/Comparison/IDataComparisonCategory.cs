using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

public interface IDataComparisonCategory<TComparison>
    where TComparison : notnull, IDataComparisonFormatterEntry
{
    string Name { get; }
    IDataComparisonCategoryResults GetResults(IEnumerable<TComparison> analyzers);
}
