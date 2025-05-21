using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Features.Comparison;

public static class DataComparison
{
    [Pure]
    public static DataComparison<TComparison> Create<TComparison>(IEnumerable<TComparison> values)
        where TComparison : IDataComparisonFormatterEntry
    {
        return new DataComparison<TComparison>(values, []);
    }

    [Pure]
    public static DataComparison<TComparison> Create<TComparison>(IEnumerable<TComparison> values, IEnumerable<IDataComparisonCategory<TComparison>> categories)
        where TComparison : IDataComparisonFormatterEntry
    {
        return new DataComparison<TComparison>(values, categories);
    }

    [Pure]
    public static DataComparison<TComparison> Create<TComparison>(IEnumerable<IDataComparisonCategory<TComparison>> categories, IEnumerable<TComparison> values)
        where TComparison : IDataComparisonFormatterEntry
    {
        return new DataComparison<TComparison>(values, categories);
    }

    [Pure]
    public static DataComparison<TComparison> Generate<TComparison, TArgs>(this DataComparison<TComparison> comparison, IEnumerable<TArgs> sourceArgs, Func<DataComparison<TComparison>, TArgs, DataComparison<TComparison>> generator)
        where TComparison : IDataComparisonFormatterEntry
    {
        foreach(var args in sourceArgs)
        {
            comparison = generator(comparison, args);
        }

        return comparison;
    }

    [Pure]
    public static DataComparison<ICalculatorWrapper<TComparison>> Generate<TComparison, TArgs>(this DataComparison<ICalculatorWrapper<TComparison>> comparison, IEnumerable<TArgs> sourceArgs, Func<DataComparison<ICalculatorWrapper<TComparison>>, TArgs, DataComparison<ICalculatorWrapper<TComparison>>> generator)
        where TComparison : ICalculator<TComparison>, IDataComparisonFormatterEntry
    {
        foreach (var args in sourceArgs)
        {
            comparison = generator(comparison, args);
        }

        return comparison;
    }

    [Pure]
    public static DataComparison<TComparison> AddCategories<TComparison>(this DataComparison<TComparison> comparison, IEnumerable<IDataComparisonCategory<TComparison>> categories)
        where TComparison : IDataComparisonFormatterEntry
    {
        foreach(var category in categories)
        {
            comparison = comparison.AddCategory(category);
        }

        return comparison;
    }

    [Pure]
    public static DataComparison<TComparison> AddCategory<TComparison>(this DataComparison<TComparison> comparison, IDataComparisonCategory<TComparison> category)
        where TComparison : IDataComparisonFormatterEntry
    {
        return new DataComparison<TComparison>(comparison, category);
    }

    [Pure]
    public static DataComparison<TComparison> AddCategory<TComparison, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, Func<TComparison, TReturn> func)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<TComparison, TReturn>(name, formatter, func);
        return new DataComparison<TComparison>(comparison, category);
    }

    [Pure]
    public static DataComparison<ICalculatorWrapper<TComparison>> AddCategory<TComparison>(this DataComparison<ICalculatorWrapper<TComparison>> comparison, string name, IFormat<double> formatter, Func<TComparison, double> func)
        where TComparison : ICalculator<TComparison>, IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<ICalculatorWrapper<TComparison>, double>(name, formatter, func.Wrap());
        return new DataComparison<ICalculatorWrapper<TComparison>>(comparison, category);
    }

    /// <summary>
    /// Create a new instance of <c>DataComparison&lt;<typeparamref name="TComparison"/>&gt;</c> with another category added to it.
    /// This is a pure function, which means it does not modify the <paramref name="comparison"/> parameter, nor any of the other arguments.
    /// </summary>
    /// <typeparam name="TComparison">The type of the object that is being compared (with other objects of the same type).</typeparam>
    /// <typeparam name="TReturn">The type of the value calculated by the category.</typeparam>
    /// <param name="comparison">The <c>DataComparison</c> to add the category.</param>
    /// <param name="name">The name of the category.</param>
    /// <param name="formatter">The string format of the value calculated by the category.</param>
    /// <param name="func">The calculator of <typeparamref name="TReturn"/> value, which will be applied to each <typeparamref name="TComparison"/>.</param>
    /// <param name="optimizer">This can take a <c>HandAnalyzer</c> can transform it into a version that is more optimized for the <c>func</c>.</param>
    /// <returns>Another instance of <c>DataComparison</c>, with a new category added. This is not the original <paramref name="comparison"/>.</returns>
    [Pure]
    public static DataComparison<TComparison> AddCategory<TComparison, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, Func<TComparison, TReturn> func, Func<TComparison, TComparison> optimizer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<TComparison, TReturn>(name, formatter, func, optimizer);
        return new DataComparison<TComparison>(comparison, category);
    }

    [Pure]
    public static DataComparison<ICalculatorWrapper<TComparison>> AddCategory<TComparison>(this DataComparison<ICalculatorWrapper<TComparison>> comparison, string name, IFormat<double> formatter, Func<TComparison, double> func, Func<TComparison, TComparison> optimizer)
        where TComparison : ICalculator<TComparison>, IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<ICalculatorWrapper<TComparison>, double>(name, formatter, calculator => calculator.Calculate(comparison =>
        {
            var optimizedVersion = optimizer(comparison);
            return func(optimizedVersion);
        }));
        return new DataComparison<ICalculatorWrapper<TComparison>>(comparison, category);
    }

    [Pure]
    public static DataComparison<TComparison> AddCategory<TComparison, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, Func<TComparison, TReturn> func, IComparer<TReturn> comparer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<TComparison, TReturn>(name, formatter, func, comparer);
        return new DataComparison<TComparison>(comparison, category);
    }

    [Pure]
    public static DataComparison<ICalculatorWrapper<TComparison>> AddCategory<TComparison>(this DataComparison<ICalculatorWrapper<TComparison>> comparison, string name, IFormat<double> formatter, Func<TComparison, double> func, IComparer<double> comparer)
        where TComparison : ICalculator<TComparison>, IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<ICalculatorWrapper<TComparison>, double>(name, formatter, func.Wrap(), comparer);
        return new DataComparison<ICalculatorWrapper<TComparison>>(comparison, category);
    }

    [Pure]
    public static DataComparison<TComparison> AddCategory<TComparison, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, Func<TComparison, TReturn> func, IComparer<TReturn> comparer, Func<TComparison, TComparison> optimizer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<TComparison, TReturn>(name, formatter, func, comparer, optimizer);
        return new DataComparison<TComparison>(comparison, category);
    }

    [Pure]
    public static DataComparison<ICalculatorWrapper<TComparison>> AddCategory<TComparison>(this DataComparison<ICalculatorWrapper<TComparison>> comparison, string name, IFormat<double> formatter, Func<TComparison, double> func, IComparer<double> comparer, Func<TComparison, TComparison> optimizer)
        where TComparison : ICalculator<TComparison>, IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<ICalculatorWrapper<TComparison>, double>(name, formatter, calculator => calculator.Calculate(comparison =>
        {
            var optimizedVersion = optimizer(comparison);
            return func(optimizedVersion);
        }), comparer);
        return new DataComparison<ICalculatorWrapper<TComparison>>(comparison, category);
    }

    [Pure]
    public static DataComparison<TComparison> AddCategory<TComparison, TArgs, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<TComparison, TArgs, TReturn> func)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<TComparison, TArgs, TReturn>(name, formatter, args, func);
        return new DataComparison<TComparison>(comparison, category);
    }

    [Pure]
    public static DataComparison<ICalculatorWrapper<TComparison>> AddCategory<TComparison, TArgs>(this DataComparison<ICalculatorWrapper<TComparison>> comparison, string name, IFormat<double> formatter, TArgs args, Func<TComparison, TArgs, double> func)
        where TComparison : ICalculator<TComparison>, IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<ICalculatorWrapper<TComparison>, TArgs, double>(name, formatter, args, (calculator, args) => calculator.Calculate(args, func));
        return new DataComparison<ICalculatorWrapper<TComparison>>(comparison, category);
    }

    [Pure]
    public static DataComparison<TComparison> AddCategory<TComparison, TArgs, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<TComparison, TArgs, TReturn> func, Func<TComparison, TComparison> optimizer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<TComparison, TArgs, TReturn>(name, formatter, args, func, optimizer);
        return new DataComparison<TComparison>(comparison, category);
    }

    [Pure]
    public static DataComparison<ICalculatorWrapper<TComparison>> AddCategory<TComparison, TArgs>(this DataComparison<ICalculatorWrapper<TComparison>> comparison, string name, IFormat<double> formatter, TArgs args, Func<TComparison, TArgs, double> func, Func<TComparison, TComparison> optimizer)
        where TComparison : ICalculator<TComparison>, IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategory<ICalculatorWrapper<TComparison>, TArgs, double>(name, formatter, args, (calculator, args) => calculator.Calculate(args, (comparison, args) =>
        {
            var optimizedVersion = optimizer(comparison);
            return func(optimizedVersion, args);
        }));
        return new DataComparison<ICalculatorWrapper<TComparison>>(comparison, category);
    }

    [Pure]
    public static DataComparison<TComparison> AddCategory<TComparison, TArgs, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<TComparison, TArgs, TReturn> func, IComparer<TReturn> comparer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<TComparison, TArgs, TReturn>(name, formatter, args, func, comparer);
        return new DataComparison<TComparison>(comparison, category);
    }

    [Pure]
    public static DataComparison<ICalculatorWrapper<TComparison>> AddCategory<TComparison, TArgs>(this DataComparison<ICalculatorWrapper<TComparison>> comparison, string name, IFormat<double> formatter, TArgs args, Func<TComparison, TArgs, double> func, IComparer<double> comparer)
        where TComparison : ICalculator<TComparison>, IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<ICalculatorWrapper<TComparison>, TArgs, double>(name, formatter, args, (calculator, args) => calculator.Calculate(args, func), comparer);
        return new DataComparison<ICalculatorWrapper<TComparison>>(comparison, category);
    }

    [Pure]
    public static DataComparison<TComparison> AddCategory<TComparison, TArgs, TReturn>(this DataComparison<TComparison> comparison, string name, IFormat<TReturn> formatter, TArgs args, Func<TComparison, TArgs, TReturn> func, IComparer<TReturn> comparer, Func<TComparison, TComparison> optimizer)
        where TComparison : IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<TComparison, TArgs, TReturn>(name, formatter, args, func, comparer, optimizer);
        return new DataComparison<TComparison>(comparison, category);
    }

    [Pure]
    public static DataComparison<ICalculatorWrapper<TComparison>> AddCategory<TComparison, TArgs>(this DataComparison<ICalculatorWrapper<TComparison>> comparison, string name, IFormat<double> formatter, TArgs args, Func<TComparison, TArgs, double> func, IComparer<double> comparer, Func<TComparison, TComparison> optimizer)
        where TComparison : ICalculator<TComparison>, IDataComparisonFormatterEntry
    {
        var category = new DataComparisonCategoryRanked<ICalculatorWrapper<TComparison>, TArgs, double>(name, formatter, args, (calculator, args) => calculator.Calculate(args, (comparison, args) =>
        {
            var optimizedVersion = optimizer(comparison);
            return func(optimizedVersion, args);
        }), comparer);
        return new DataComparison<ICalculatorWrapper<TComparison>>(comparison, category);
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

    public IDataComparisonFormatter Run(CreateDataComparisonFormatter factory)
    {
        var results = new List<IDataComparisonCategoryResults>();

        foreach (var category in Categories)
        {
            results.Add(category.GetResults(ComparisonFocuses));
        }

        return factory(ComparisonFocuses.Cast<IDataComparisonFormatterEntry>(), results);
    }

    public IDataComparisonFormatter<T> Run<T>(CreateDataComparisonFormatter<T> factory)
        where T : notnull
    {
        var results = new List<IDataComparisonCategoryResults>();

        foreach (var category in Categories)
        {
            results.Add(category.GetResults(ComparisonFocuses));
        }

        return factory(ComparisonFocuses.Cast<IDataComparisonFormatterEntry>(), results);
    }

    public IDataComparisonFormatter RunInParallel(CreateDataComparisonFormatter factory)
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
        return factory(ComparisonFocuses.Cast<IDataComparisonFormatterEntry>(), results);
    }
}
