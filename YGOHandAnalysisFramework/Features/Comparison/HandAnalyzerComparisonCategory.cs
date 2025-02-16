using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

public static class HandAnalyzerComparisonCategory
{
    /// <summary>
    /// Add a category to a <c>HandAnalyzerComparison</c>. Each category is projected, or applied, to each <c>HandAnalyzer</c>.
    /// </summary>
    /// <typeparam name="TCardGroup">The type of the card group. This has the card data (name, quantity, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The type of the name used in <c>TCardGroup</c>.</typeparam>
    /// <typeparam name="TReturn">The type of the value calculated by the category.</typeparam>
    /// <param name="comparison">The <c>HandAnalyzerComparison</c> to add the category.</param>
    /// <param name="name">The name of the category.</param>
    /// <param name="formatter">The string format of the value calculated by the category.</param>
    /// <param name="func">The calculator of <c>TReturn</c> value, which will be applied to each <c>HandAnalyzer</c>.</param>
    /// <returns>The <c>HandAnalyzerComparison</c> provided.</returns>
    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IFormat<TReturn> formatter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn>(name, formatter, func);
        comparison.Add(category);
        return comparison;
    }

    /// <summary>
    /// Add a category to a <c>HandAnalyzerComparison</c>. Each category is projected, or applied, to each <c>HandAnalyzer</c>.
    /// </summary>
    /// <typeparam name="TCardGroup">The type of the card group. This has the card data (name, quantity, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The type of the name used in <c>TCardGroup</c>.</typeparam>
    /// <typeparam name="TReturn">The type of the value calculated by the category.</typeparam>
    /// <param name="comparison">The <c>HandAnalyzerComparison</c> to add the category.</param>
    /// <param name="name">The name of the category.</param>
    /// <param name="formatter">The string format of the value calculated by the category.</param>
    /// <param name="func">The calculator of <c>TReturn</c> value, which will be applied to each <c>HandAnalyzer</c>.</param>
    /// <param name="optimizer">This can take a <c>HandAnalyzer</c> can transform it into a version that is more optimized for the <c>func</c>. Typically, this means it will have less <c>CardGroup</c>s, which means less <c>HandCombination</c> objects to enumerate over.</param>
    /// <returns>The <c>HandAnalyzerComparison</c> provided.</returns>
    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IFormat<TReturn> formatter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func, HandAnalyzerOptimizer<TCardGroup, TCardGroupName> optimizer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn>(name, formatter, func, optimizer);
        comparison.Add(category);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IFormat<TReturn> formatter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func, IComparer<TReturn> comparer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategoryRanked<TCardGroup, TCardGroupName, TReturn>(name, formatter, func, comparer);
        comparison.Add(category);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IFormat<TReturn> formatter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func, IComparer<TReturn> comparer, HandAnalyzerOptimizer<TCardGroup, TCardGroupName> optimizer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategoryRanked<TCardGroup, TCardGroupName, TReturn>(name, formatter, func, comparer, optimizer);
        comparison.Add(category);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TArgs, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TArgs, TReturn>(name, formatter, args, func);
        comparison.Add(category);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TArgs, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func, HandAnalyzerOptimizer<TCardGroup, TCardGroupName> optimizer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TArgs, TReturn>(name, formatter, args, func, optimizer);
        comparison.Add(category);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TArgs, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func, IComparer<TReturn> comparer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategoryRanked<TCardGroup, TCardGroupName, TArgs, TReturn>(name, formatter, args, func, comparer);
        comparison.Add(category);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName, TArgs, TReturn>(this HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func, IComparer<TReturn> comparer, HandAnalyzerOptimizer<TCardGroup, TCardGroupName> optimizer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var category = new HandAnalyzerComparisonCategoryRanked<TCardGroup, TCardGroupName, TArgs, TReturn>(name, formatter, args, func, comparer, optimizer);
        comparison.Add(category);
        return comparison;
    }
}

internal sealed class HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn> : HandAnalyzerComparisonCategoryBase<TCardGroup, TCardGroupName, TReturn>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> Function { get; }

    public HandAnalyzerComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func,
        HandAnalyzerOptimizer<TCardGroup, TCardGroupName> optimizer)
        : base(name, formatter, optimizer)
    {
        Function = func;
    }

    public HandAnalyzerComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> func)
        : this(name, formatter, func, static analyzer => analyzer) { }

    protected override TReturn Run(HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
    {
        return Function(analyzer);
    }

    protected override IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName> CollateResults(IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> results, TimeSpan executionTime)
    {
        return new HandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName, TReturn>(Name, Format, results, executionTime);
    }
}

internal sealed class HandAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TArgs, TReturn> : HandAnalyzerComparisonCategoryBase<TCardGroup, TCardGroupName, TReturn>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private TArgs Args { get; }
    private Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> Function { get; }

    public HandAnalyzerComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        TArgs args,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func,
        HandAnalyzerOptimizer<TCardGroup, TCardGroupName> optimizer)
        : base(name, formatter, optimizer)
    {
        Function = func;
        Args = args;
    }

    public HandAnalyzerComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        TArgs args,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, TReturn> func)
        : this(name, formatter, args, func, static analyzer => analyzer) { }

    protected override TReturn Run(HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
    {
        return Function(analyzer, Args);
    }

    protected override IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName> CollateResults(IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> results, TimeSpan executionTime)
    {
        return new HandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName, TReturn>(Name, Format, results, executionTime);
    }
}
