namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public interface IDataComparisonFormatterFactory
{
    IDataComparisonFormatter CreateFormatter(IEnumerable<IDataComparisonFormatterEntry> handAnalyzers, IEnumerable<IDataComparisonCategoryResults> categoryResults);
}
