using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

internal sealed class DataComparisonCategory<TComparison, TReturn> : DataComparisonCategoryBase<TComparison, TReturn>
    where TComparison : notnull, IDataComparisonFormatterEntry
{
    private Func<TComparison, TReturn> Function { get; }

    public DataComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        Func<TComparison, TReturn> func,
        Func<TComparison, TComparison> optimizer)
        : base(name, formatter, optimizer)
    {
        Function = func;
    }

    public DataComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        Func<TComparison, TReturn> func)
        : this(name, formatter, func, static analyzer => analyzer) { }

    protected override TReturn Run(TComparison analyzer)
    {
        return Function(analyzer);
    }

    protected override IDataComparisonCategoryResults CollateResults(IReadOnlyDictionary<TComparison, TReturn> results, TimeSpan executionTime)
    {
        return DataComparisonCategoryResults.Create(Name, Format, results, executionTime);
    }
}

internal sealed class DataComparisonCategory<TComparison, TArgs, TReturn> : DataComparisonCategoryBase<TComparison, TReturn>
    where TComparison : notnull, IDataComparisonFormatterEntry
{
    private TArgs Args { get; }
    private Func<TComparison, TArgs, TReturn> Function { get; }

    public DataComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        TArgs args,
        Func<TComparison, TArgs, TReturn> func,
        Func<TComparison, TComparison> optimizer)
        : base(name, formatter, optimizer)
    {
        Function = func;
        Args = args;
    }

    public DataComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        TArgs args,
        Func<TComparison, TArgs, TReturn> func)
        : this(name, formatter, args, func, static analyzer => analyzer) { }

    protected override TReturn Run(TComparison analyzer)
    {
        return Function(analyzer, Args);
    }

    protected override IDataComparisonCategoryResults CollateResults(IReadOnlyDictionary<TComparison, TReturn> results, TimeSpan executionTime)
    {
        return DataComparisonCategoryResults.Create(Name, Format, results, executionTime);
    }
}
