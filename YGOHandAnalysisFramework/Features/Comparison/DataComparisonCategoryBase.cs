using System.Diagnostics;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

internal abstract class DataComparisonCategoryBase<TComparison, TReturn> : IDataComparisonCategory<TComparison>
    where TComparison : notnull, IDataComparisonFormatterEntry
{
    protected IFormat<TReturn> Format { get; }
    protected Func<TComparison, TComparison> Optimizer { get; }

    public string Name { get; }

    protected DataComparisonCategoryBase(string name, IFormat<TReturn> format, Func<TComparison, TComparison> optimizer)
    {
        Name = name;
        Format = format;
        Optimizer = optimizer;
    }

    protected abstract TReturn Run(TComparison analyzer);

    protected abstract IDataComparisonCategoryResults CollateResults(IReadOnlyDictionary<TComparison, TReturn> results, TimeSpan executionTime);

    public IDataComparisonCategoryResults GetResults(IEnumerable<TComparison> analyzers)
    {
        var results = new Dictionary<TComparison, TReturn>();

        var sw = Stopwatch.StartNew();
        foreach (var analyzer in analyzers)
        {
            var optimizedAnalyzer = Optimizer(analyzer);
            results.Add(analyzer, Run(optimizedAnalyzer));
        }
        sw.Stop();

        return CollateResults(results, sw.Elapsed);
    }
}
