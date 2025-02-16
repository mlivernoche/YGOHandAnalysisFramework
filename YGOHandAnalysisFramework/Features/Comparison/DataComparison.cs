using System.Collections;
using System.Collections.Concurrent;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

public static class DataComparison
{
    public static DataComparison<TComparison> Create<TComparison>(IEnumerable<TComparison> values)
        where TComparison : IDataComparisonFormatterEntry
    {
        return new DataComparison<TComparison>(values, []);
    }

    public static DataComparison<TComparison> Create<TComparison>(IEnumerable<TComparison> values, IEnumerable<IDataComparisonCategory<TComparison>> categories)
        where TComparison : IDataComparisonFormatterEntry
    {
        return new DataComparison<TComparison>(values, categories);
    }

    public static DataComparison<TComparison> Create<TComparison>(IEnumerable<IDataComparisonCategory<TComparison>> categories, IEnumerable<TComparison> values)
        where TComparison : IDataComparisonFormatterEntry
    {
        return new DataComparison<TComparison>(values, categories);
    }

    public static DataComparison<TComparison> Add<TComparison>(this DataComparison<TComparison> comparison, IEnumerable<IDataComparisonCategory<TComparison>> categories)
        where TComparison : IDataComparisonFormatterEntry
    {
        foreach(var category in categories)
        {
            comparison = comparison.Add(category);
        }

        return comparison;
    }

    public static DataComparison<TComparison> Add<TComparison>(this DataComparison<TComparison> comparison, IDataComparisonCategory<TComparison> category)
        where TComparison : IDataComparisonFormatterEntry
    {
        return new DataComparison<TComparison>(comparison, category);
    }

    /// <summary>
    /// Add a category to a <c>IDataComparison</c>. Each category is projected, or applied, to each <c>TComparison</c>.
    /// This method is does not modify the input.
    /// </summary>
    /// <typeparam name="TComparison">The type of the object that is being compared (with other objects of the same type).</typeparam>
    /// <typeparam name="TReturn">The type of the value calculated by the category.</typeparam>
    /// <param name="comparison">The <c>HandAnalyzerComparison</c> to add the category.</param>
    /// <param name="name">The name of the category.</param>
    /// <param name="formatter">The string format of the value calculated by the category.</param>
    /// <param name="func">The calculator of <c>TReturn</c> value, which will be applied to each <c>TComparison</c>.</param>
    /// <returns>The <c>HandAnalyzerComparison</c> provided.</returns>
    public static DataComparison<TComparison> Add<TComparison, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, Func<TComparison, TReturn> func)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<TComparison, TReturn>(name, formatter, func);
        return new DataComparison<TComparison>(comparison, category);
    }

    /// <summary>
    /// Add a category to a <c>HandAnalyzerComparison</c>. Each category is projected, or applied, to each <c>HandAnalyzer</c>.
    /// </summary>
    /// <typeparam name="TComparison">The type of the object that is being compared (with other objects of the same type).</typeparam>
    /// <typeparam name="TReturn">The type of the value calculated by the category.</typeparam>
    /// <param name="comparison">The <c>HandAnalyzerComparison</c> to add the category.</param>
    /// <param name="name">The name of the category.</param>
    /// <param name="formatter">The string format of the value calculated by the category.</param>
    /// <param name="func">The calculator of <c>TReturn</c> value, which will be applied to each <c>HandAnalyzer</c>.</param>
    /// <param name="optimizer">This can take a <c>HandAnalyzer</c> can transform it into a version that is more optimized for the <c>func</c>. Typically, this means it will have less <c>CardGroup</c>s, which means less <c>HandCombination</c> objects to enumerate over.</param>
    /// <returns>The <c>HandAnalyzerComparison</c> provided.</returns>
    public static DataComparison<TComparison> Add<TComparison, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, Func<TComparison, TReturn> func, Func<TComparison, TComparison> optimizer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<TComparison, TReturn>(name, formatter, func, optimizer);
        return new DataComparison<TComparison>(comparison, category);
    }

    public static DataComparison<TComparison> Add<TComparison, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, Func<TComparison, TReturn> func, IComparer<TReturn> comparer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<TComparison, TReturn>(name, formatter, func, comparer);
        return new DataComparison<TComparison>(comparison, category);
    }

    public static DataComparison<TComparison> Add<TComparison, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, Func<TComparison, TReturn> func, IComparer<TReturn> comparer, Func<TComparison, TComparison> optimizer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<TComparison, TReturn>(name, formatter, func, comparer, optimizer);
        return new DataComparison<TComparison>(comparison, category);
    }

    public static DataComparison<TComparison> Add<TComparison, TArgs, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<TComparison, TArgs, TReturn> func)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<TComparison, TArgs, TReturn>(name, formatter, args, func);
        return new DataComparison<TComparison>(comparison, category);
    }

    public static DataComparison<TComparison> Add<TComparison, TArgs, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<TComparison, TArgs, TReturn> func, Func<TComparison, TComparison> optimizer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<TComparison, TArgs, TReturn>(name, formatter, args, func, optimizer);
        return new DataComparison<TComparison>(comparison, category);
    }

    public static DataComparison<TComparison> Add<TComparison, TArgs, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<TComparison, TArgs, TReturn> func, IComparer<TReturn> comparer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<TComparison, TArgs, TReturn>(name, formatter, args, func, comparer);
        return new DataComparison<TComparison>(comparison, category);
    }

    public static DataComparison<TComparison> Add<TComparison, TArgs, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<TComparison, TArgs, TReturn> func, IComparer<TReturn> comparer, Func<TComparison, TComparison> optimizer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<TComparison, TArgs, TReturn>(name, formatter, args, func, comparer, optimizer);
        return new DataComparison<TComparison>(comparison, category);
    }
}

public class DataComparison<TComparison>
    where TComparison : IDataComparisonFormatterEntry
{
    public IEnumerable<TComparison> ComparisonFocuses { get; }
    public IEnumerable<IDataComparisonCategory<TComparison>> Categories { get; }

    public DataComparison(IEnumerable<TComparison> comparisonFocuses, IEnumerable<IDataComparisonCategory<TComparison>> categories)
    {
        ComparisonFocuses = new HashSet<TComparison>(comparisonFocuses);
        Categories = new HashSet<IDataComparisonCategory<TComparison>>(categories);
    }

    internal DataComparison(DataComparison<TComparison> original, IDataComparisonCategory<TComparison> categories)
    {
        ComparisonFocuses = new HashSet<TComparison>(original.ComparisonFocuses);
        Categories = new HashSet<IDataComparisonCategory<TComparison>>([..original.Categories, categories]);
    }

    internal DataComparison(DataComparison<TComparison> original, IEnumerable<IDataComparisonCategory<TComparison>> categories)
    {
        ComparisonFocuses = new HashSet<TComparison>(original.ComparisonFocuses);
        Categories = new HashSet<IDataComparisonCategory<TComparison>>([.. original.Categories, ..categories]);
    }

    public IDataComparisonFormatter Run(CreateDataComparisonFormat createDataComparisonFormat)
    {
        var results = new List<IDataComparisonCategoryResults>();

        foreach (var category in Categories)
        {
            results.Add(category.GetResults(ComparisonFocuses));
        }

        return createDataComparisonFormat(ComparisonFocuses.Cast<IDataComparisonFormatterEntry>(), results);
    }

    public IDataComparisonFormatter RunInParallel(CreateDataComparisonFormat createDataComparisonFormat)
    {
        var list = new List<(int, IDataComparisonCategory<TComparison>)>();

        {
            int sortId = 0;
            foreach (var category in Categories)
            {
                list.Add((sortId++, category));
            }
        }

        var output = new ConcurrentBag<(int SortId, IDataComparisonCategoryResults Result)>();

        Parallel.ForEach(list, tuple =>
        {
            var (sortId, category) = tuple;
            output.Add((sortId, category.GetResults(ComparisonFocuses)));
        });

        var results = output
            .OrderBy(static x => x.SortId)
            .Select(static x => x.Result)
            .ToList();
        return createDataComparisonFormat(ComparisonFocuses.Cast<IDataComparisonFormatterEntry>(), results);
    }
}
