using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

internal sealed class DataComparisonCategoryRanked<TComparison, TReturn> : DataComparisonCategoryBase<TComparison, TReturn>
    where TComparison : notnull, IDataComparisonFormatterEntry
{
    private Func<TComparison, TReturn> Function { get; }
    private IComparer<TReturn> Comparer { get; }

    public DataComparisonCategoryRanked(
        string name,
        IFormat<TReturn> formatter,
        Func<TComparison, TReturn> func,
        IComparer<TReturn> comparer,
        Func<TComparison, TComparison> optimizer)
        : base(name, formatter, optimizer)
    {
        Function = func;
        Comparer = comparer;
    }

    public DataComparisonCategoryRanked(
        string name,
        IFormat<TReturn> formatter,
        Func<TComparison, TReturn> func,
        IComparer<TReturn> comparer)
        : this(name, formatter, func, comparer, static analyzer => analyzer) { }

    protected override TReturn Run(TComparison analyzer)
    {
        return Function(analyzer);
    }

    protected override IDataComparisonCategoryResults CollateResults(IReadOnlyDictionary<TComparison, TReturn> results, TimeSpan executionTime)
    {
        return DataComparisonCategoryResultsRanked.Create(Name, Format, results, executionTime, Comparer);
    }
}

internal sealed class DataComparisonCategoryRanked<TComparison, TArgs, TReturn> : DataComparisonCategoryBase<TComparison, TReturn>
    where TComparison : notnull, IDataComparisonFormatterEntry
{
    private TArgs Args { get; }
    private Func<TComparison, TArgs, TReturn> Function { get; }
    private IComparer<TReturn> Comparer { get; }

    public DataComparisonCategoryRanked(
        string name,
        IFormat<TReturn> formatter,
        TArgs args,
        Func<TComparison, TArgs, TReturn> func,
        IComparer<TReturn> comparer,
        Func<TComparison, TComparison> optimizer)
        : base(name, formatter, optimizer)
    {
        Args = args;
        Function = func;
        Comparer = comparer;
    }

    public DataComparisonCategoryRanked(
        string name,
        IFormat<TReturn> formatter,
        TArgs args,
        Func<TComparison, TArgs, TReturn> func,
        IComparer<TReturn> comparer)
        : this(name, formatter, args, func, comparer, static analyzer => analyzer) { }

    protected override TReturn Run(TComparison analyzer)
    {
        return Function(analyzer, Args);
    }

    protected override IDataComparisonCategoryResults CollateResults(IReadOnlyDictionary<TComparison, TReturn> results, TimeSpan executionTime)
    {
        return DataComparisonCategoryResultsRanked.Create(Name, Format, results, executionTime, Comparer);
    }
}
