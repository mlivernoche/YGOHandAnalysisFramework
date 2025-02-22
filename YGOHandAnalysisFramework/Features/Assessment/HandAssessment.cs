﻿using System.Diagnostics.Contracts;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;

namespace YGOHandAnalysisFramework.Features.Assessment;

public static class HandAssessment
{
    [Pure]
    public static DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> AddAssessment<TCardGroup, TCardGroupName, TReturn, TAssessment>(
        this DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> comparison,
        string categoryName,
        IFormat<TReturn> formatter,
        Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var category = new HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment>(categoryName, formatter, assessmentFactory, func, new());
        return comparison.AddCategory(category);
    }

    [Pure]
    public static DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> AddAssessment<TCardGroup, TCardGroupName, TReturn, TAssessment>(
        this DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> comparison,
        string categoryName,
        IFormat<TReturn> formatter,
        Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> func,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>> optimizer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var category = new HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment>(categoryName, formatter, assessmentFactory, func, new(), optimizer);
        return comparison.AddCategory(category);
    }

    [Pure]
    public static DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> AddAssessment<TCardGroup, TCardGroupName, TAssessment>(
        this DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> comparison,
        string categoryName,
        IFormat<double> formatter,
        Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        Func<TAssessment, bool> predicate)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var category = new HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, double, TAssessment>(categoryName, formatter, assessmentFactory, analyzer => analyzer.CalculateProbability(predicate), new());
        return comparison.AddCategory(category);
    }

    [Pure]
    public static DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> AddAssessment<TCardGroup, TCardGroupName, TReturn, TAssessment>(
        this DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> comparison,
        IFormat<TReturn> formatter,
        Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        params (string Name, Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> Method)[] func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var cache = new AssessmentCache<TCardGroup, TCardGroupName, TAssessment>();

        foreach (var (Name, Method) in func)
        {
            var category = new HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment>(Name, formatter, assessmentFactory, Method, cache);
            comparison = comparison.AddCategory(category);
        }

        return comparison;
    }

    [Pure]
    public static DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> AddAssessment<TCardGroup, TCardGroupName, TAssessment>(
        this DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> comparison,
        IFormat<double> formatter,
        Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        params (string Name, Func<TAssessment, bool> Predicate)[] func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var cache = new AssessmentCache<TCardGroup, TCardGroupName, TAssessment>();

        foreach (var calculator in func)
        {
            var category = new HandAssessmentComparisonCategory<TCardGroup, TCardGroupName, double, TAssessment>(calculator.Name, formatter, assessmentFactory, analyzer => analyzer.CalculateProbability(calculator.Predicate), cache);
            comparison = comparison.AddCategory(category);
        }

        return comparison;
    }

    [Pure]
    public static DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> AddAssessment<TCardGroup, TCardGroupName, TReturn, TAssessment>(
        this DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> comparison,
        string categoryName,
        IFormat<TReturn> formatter,
        Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> assessmentFactory,
        Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var category = new HandAssessmentWithAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment>(categoryName, formatter, assessmentFactory, func, new());
        return comparison.AddCategory(category);
    }

    [Pure]
    public static DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> AddAssessment<TCardGroup, TCardGroupName, TAssessment>(
        this DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> comparison,
        string categoryName,
        IFormat<double> formatter,
        Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> assessmentFactory,
        Func<TAssessment, bool> predicate)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var category = new HandAssessmentWithAnalyzerComparisonCategory<TCardGroup, TCardGroupName, double, TAssessment>(categoryName, formatter, assessmentFactory, analyzer => analyzer.CalculateProbability(predicate), new());
        return comparison.AddCategory(category);
    }

    [Pure]
    public static DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> AddAssessment<TCardGroup, TCardGroupName, TReturn, TAssessment>(
        this DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> comparison,
        IFormat<TReturn> formatter,
        Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> assessmentFactory,
        params (string Name, Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> Method)[] func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var cache = new AssessmentCache<TCardGroup, TCardGroupName, TAssessment>();

        foreach (var (Name, Method) in func)
        {
            var category = new HandAssessmentWithAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment>(Name, formatter, assessmentFactory, Method, cache);
            comparison = comparison.AddCategory(category);
        }

        return comparison;
    }

    [Pure]
    public static DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> AddAssessment<TCardGroup, TCardGroupName, TAssessment>(
        this DataComparison<HandAnalyzer<TCardGroup, TCardGroupName>> comparison,
        IFormat<double> formatter,
        Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> assessmentFactory,
        params (string Name, Func<TAssessment, bool> Predicate)[] func)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var cache = new AssessmentCache<TCardGroup, TCardGroupName, TAssessment>();

        foreach (var calculator in func)
        {
            var category = new HandAssessmentWithAnalyzerComparisonCategory<TCardGroup, TCardGroupName, double, TAssessment>(calculator.Name, formatter, assessmentFactory, analyzer => analyzer.CalculateProbability(calculator.Predicate), cache);
            comparison = comparison.AddCategory(category);
        }

        return comparison;
    }
}
