using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

internal sealed class HandAnalyzerComparisonCategoryRanked<TCardGroup, TCardGroupName, TReturn> : HandAnalyzerComparisonCategoryBase<TCardGroup, TCardGroupName, TReturn>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> Function { get; }
    private IComparer<TReturn> Comparer { get; }

    public HandAnalyzerComparisonCategoryRanked(
        string name,
        IFormat<TReturn> formatter,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func,
        IComparer<TReturn> comparer,
        HandAnalyzerOptimizer<TCardGroup, TCardGroupName> optimizer)
        : base(name, formatter, optimizer)
    {
        Function = func;
        Comparer = comparer;
    }

    public HandAnalyzerComparisonCategoryRanked(
        string name,
        IFormat<TReturn> formatter,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func,
        IComparer<TReturn> comparer)
        : this(name, formatter, func, comparer, static analyzer => analyzer) { }

    protected override TReturn Run(HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
    {
        return Function(analyzer);
    }

    protected override IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName> CollateResults(IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> results, TimeSpan executionTime)
    {
        return new HandAnalyzerComparisonCategoryResultsRanked<TCardGroup, TCardGroupName, TReturn>(Name, Format, results, executionTime, Comparer);
    }
}

internal sealed class HandAnalyzerComparisonCategoryRanked<TCardGroup, TCardGroupName, TArgs, TReturn> : HandAnalyzerComparisonCategoryBase<TCardGroup, TCardGroupName, TReturn>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private TArgs Args { get; }
    private Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> Function { get; }
    private IComparer<TReturn> Comparer { get; }

    public HandAnalyzerComparisonCategoryRanked(
        string name,
        IFormat<TReturn> formatter,
        TArgs args,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func,
        IComparer<TReturn> comparer,
        HandAnalyzerOptimizer<TCardGroup, TCardGroupName> optimizer)
        : base(name, formatter, optimizer)
    {
        Args = args;
        Function = func;
        Comparer = comparer;
    }

    public HandAnalyzerComparisonCategoryRanked(
        string name,
        IFormat<TReturn> formatter,
        TArgs args,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func,
        IComparer<TReturn> comparer)
        : this(name, formatter, args, func, comparer, static analyzer => analyzer) { }

    protected override TReturn Run(HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
    {
        return Function(analyzer, Args);
    }

    protected override IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName> CollateResults(IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> results, TimeSpan executionTime)
    {
        return new HandAnalyzerComparisonCategoryResultsRanked<TCardGroup, TCardGroupName, TReturn>(Name, Format, results, executionTime, Comparer);
    }
}
