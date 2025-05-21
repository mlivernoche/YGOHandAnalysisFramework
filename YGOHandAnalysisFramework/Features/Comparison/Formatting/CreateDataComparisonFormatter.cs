namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public delegate IDataComparisonFormatter<T> CreateDataComparisonFormatter<T>(IEnumerable<IDataComparisonFormatterEntry> entries, IEnumerable<IDataComparisonCategoryResults> results)
    where T : notnull;
public delegate IDataComparisonFormatter CreateDataComparisonFormatter(IEnumerable<IDataComparisonFormatterEntry> entries, IEnumerable<IDataComparisonCategoryResults> results);
