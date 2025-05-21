namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public interface IDataComparisonFormatter<T> where T : notnull
{
    T FormatResults();
}

public interface IDataComparisonFormatter : IDataComparisonFormatter<string>
{
    string FormatResults();
}
