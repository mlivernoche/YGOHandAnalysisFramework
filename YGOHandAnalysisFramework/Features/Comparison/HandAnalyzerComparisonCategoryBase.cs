using System.Diagnostics;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

internal abstract class HandAnalyzerComparisonCategoryBase<TCardGroup, TCardGroupName, TReturn> : IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    protected IFormat<TReturn> Format { get; }
    protected HandAnalyzerOptimizer<TCardGroup, TCardGroupName> Optimizer { get; }

    public string Name { get; }

    protected HandAnalyzerComparisonCategoryBase(string name, IFormat<TReturn> format, HandAnalyzerOptimizer<TCardGroup, TCardGroupName> optimizer)
    {
        Name = name;
        Format = format;
        Optimizer = optimizer;
    }

    protected abstract TReturn Run(HandAnalyzer<TCardGroup, TCardGroupName> analyzer);

    protected abstract IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName> CollateResults(IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> results, TimeSpan executionTime);

    public IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName> GetResults(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers)
    {
        var results = new Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn>();

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
