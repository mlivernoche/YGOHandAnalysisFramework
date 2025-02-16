namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public interface IDataComparisonCategoryResults
{
    string Name { get; }
    TimeSpan ExecutionTime { get; }
    string GetResult(IDataComparisonFormatterEntry key);
}
