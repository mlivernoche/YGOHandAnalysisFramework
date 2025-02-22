using System;
using YGOHandAnalysisFramework.Data.Extensions;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Features.Probability;
using YGOHandAnalysisFramework.Features.WeightedProbability;

namespace YGOHandAnalysisFramework.Projects.HandComposition;

public class HandCompositionProject<TCardGroup, TCardGroupName, TCategory> : IProject
    where TCardGroup : IHandComposition<TCardGroupName, TCategory>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    where TCategory : notnull, IHandCompositionCategory
{
    protected record ExpectedValueFunctionContext(TCategory Category, IEqualityComparer<TCategory> EqualityComparer);

    private IEnumerable<WeightedProbabilityCollection<HandAnalyzer<TCardGroup, TCardGroupName>>> WeightedProbabilities { get; }
    private IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> HandAnalyzers { get; }
    private IEnumerable<TCategory> HandCompositionCategories { get; }
    private IEqualityComparer<TCategory> EqualityComparer { get; }
    private CreateDataComparisonFormat DataComparisonFormatFactory { get; }

    public HandCompositionProject(
        IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers,
        IEnumerable<TCategory> categories,
        IEqualityComparer<TCategory> equalityComparer,
        CreateDataComparisonFormat dataComparisonFormatFactory)
        : this(handAnalyzers, categories, equalityComparer, [], dataComparisonFormatFactory)
    {
    }

    public HandCompositionProject(
        IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers,
        IEnumerable<TCategory> categories,
        IEqualityComparer<TCategory> equalityComparer,
        IEnumerable<WeightedProbabilityCollection<HandAnalyzer<TCardGroup, TCardGroupName>>> weightedProbabilities,
        CreateDataComparisonFormat dataComparisonFormatFactory)
    {
        HandAnalyzers = handAnalyzers;
        HandCompositionCategories = categories;
        EqualityComparer = equalityComparer;
        WeightedProbabilities = weightedProbabilities;
        DataComparisonFormatFactory = dataComparisonFormatFactory ?? throw new ArgumentNullException(nameof(dataComparisonFormatFactory));
    }

    public string ProjectName { get; } = $"{nameof(HandCompositionProject<TCardGroup, TCardGroupName, TCategory>)}<{typeof(TCardGroup).FullName}, {typeof(TCardGroupName).FullName}, {typeof(TCategory).FullName}>";

    public void Run(IHandAnalyzerOutputStream outputStream)
    {
        // Not worrying about duplicates
        CompareIncludingDuplicates(HandAnalyzers)
            .RunInParallel(DataComparisonFormatFactory)
            .FormatResults()
            .Write(outputStream);
        CompareIncludingDuplicates(WeightedProbabilities)
            .RunInParallel(DataComparisonFormatFactory)
            .FormatResults()
            .Write(outputStream);

        // Worrying about duplicates
        CompareWithoutIncludingDuplicates(HandAnalyzers)
            .RunInParallel(DataComparisonFormatFactory)
            .FormatResults()
            .Write(outputStream);
        CompareWithoutIncludingDuplicates(WeightedProbabilities)
            .RunInParallel(DataComparisonFormatFactory)
            .FormatResults()
            .Write(outputStream);

        // Efficiencies
        {
            var comparison = DataComparison.Create(HandAnalyzers);

            foreach (var compositionCategory in HandCompositionCategories)
            {
                comparison = comparison.AddCategory(compositionCategory.Name, PercentFormat<double>.Default, new ExpectedValueFunctionContext(compositionCategory, EqualityComparer), static (analyzer, context) =>
                {
                    var ev = analyzer.CalculateExpectedValue(context, CalculateExpectedValueOfCategory);
                    var effectiveEV = analyzer.CalculateExpectedValue(context, CalculateEffectiveExpectedValueOfCategory);

                    if (ev == 0.0)
                    {
                        return 0.0;
                    }

                    return effectiveEV / ev;
                },
                compositionCategory.ValueComparer);
            }

            var results = comparison
                .AddCategory("Effective Hand Size", PercentFormat<double>.Default, (HandCompositionCategories, EqualityComparer), static (analyzer, context) =>
                {
                    var handSize = analyzer.HandSize;
                    var effectiveHandSize = CalculateEffectiveHandSize(analyzer, context);

                    if (handSize == 0.0)
                    {
                        return 0.0;
                    }

                    return effectiveHandSize / handSize;
                },
                HandAnalyzerComparison.GetDescendingComparer<double>())
                .AddCategory("Net Effective Hand Size", PercentFormat<double>.Default, (HandCompositionCategories, EqualityComparer), static (analyzer, context) =>
                {
                    var handSize = analyzer.HandSize;
                    var effectiveHandSize = CalculateEffectiveHandSizeWithCardEconomy(analyzer, context);

                    if (handSize == 0.0)
                    {
                        return 0.0;
                    }

                    return effectiveHandSize / handSize;
                },
                HandAnalyzerComparison.GetDescendingComparer<double>())
                .RunInParallel(DataComparisonFormatFactory);
            outputStream.Write(results.FormatResults());
        }
    }

    private DataComparison<TComparison> CompareIncludingDuplicates<TComparison>(IEnumerable<TComparison> objectsToCompare)
        where TComparison : IDataComparisonFormatterEntry, ICalculator<HandAnalyzer<TCardGroup, TCardGroupName>>
    {
        static double CalculateExpectedValue(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, ExpectedValueFunctionContext context)
        {
            return analyzer.CalculateExpectedValue(context, static (analyzer, context, hand) =>
            {
                var included = 0;
                var (comparer, category) = (context.EqualityComparer, context.Category);

                foreach (var card in hand.GetCardsInHand(analyzer))
                {
                    if (comparer.Equals(card.Category, category))
                    {
                        included += hand.CountCopiesOfCardInHand(card.Name);
                    }
                }

                return included;
            });
        }

        var comparison = DataComparison.Create(objectsToCompare);

        foreach (var compositionCategory in HandCompositionCategories)
        {
            comparison = comparison.AddCategory(compositionCategory.Name, CardinalFormat<double>.Default, new ExpectedValueFunctionContext(compositionCategory, EqualityComparer), static (analyzer, context) => analyzer.Calculate(context, CalculateExpectedValue), compositionCategory.ValueComparer);
        }

        return comparison
            .AddCategory("Starting Hand Size", CardinalFormat<double>.Default, static weighted => weighted.Calculate(static analyzer => analyzer.HandSize), HandAnalyzerComparison.GetDescendingComparer<double>());
    }

    private DataComparison<TComparison> CompareWithoutIncludingDuplicates<TComparison>(IEnumerable<TComparison> objectsToCompare)
        where TComparison : IDataComparisonFormatterEntry, ICalculator<HandAnalyzer<TCardGroup, TCardGroupName>>
    {
        static double CalculateExpectedValue(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, ExpectedValueFunctionContext context)
        {
            return analyzer.CalculateExpectedValue(context, static (analyzer, context, hand) =>
            {
                var included = 0;

                foreach (var card in hand.GetCardsInHand(analyzer))
                {
                    if (context.EqualityComparer.Equals(card.Category, context.Category))
                    {
                        included += hand.CountEffectiveCopies(card);
                    }
                }

                return included;
            });
        }

        var comparison = DataComparison.Create(objectsToCompare);

        foreach (var compositionCategory in HandCompositionCategories)
        {
            comparison = comparison.AddCategory(compositionCategory.Name, CardinalFormat<double>.Default, new ExpectedValueFunctionContext(compositionCategory, EqualityComparer), static (analyzer, context) => analyzer.Calculate(context, CalculateExpectedValue), compositionCategory.ValueComparer);
        }

        return comparison
            .AddCategory("Effective Hand Size", CardinalFormat<double>.Default, (HandCompositionCategories, EqualityComparer), static (analyzer, context) => analyzer.Calculate(context, CalculateEffectiveHandSize), HandAnalyzerComparison.GetDescendingComparer<double>())
            .AddCategory("Net Effective Hand Size", CardinalFormat<double>.Default, (HandCompositionCategories, EqualityComparer), static (analyzer, context) => analyzer.Calculate(context, CalculateEffectiveHandSizeWithCardEconomy), HandAnalyzerComparison.GetDescendingComparer<double>());
    }

    private static double CalculateExpectedValueOfCategory(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, ExpectedValueFunctionContext context, HandCombination<TCardGroupName> hand)
    {
        var included = 0;
        var (comparer, category) = (context.EqualityComparer, context.Category);

        foreach (var card in hand.GetCardsInHand(analyzer))
        {
            if (comparer.Equals(card.Category, category))
            {
                included += hand.CountCopiesOfCardInHand(card.Name);
            }
        }

        return included;
    }

    private static double CalculateEffectiveExpectedValueOfCategory(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, ExpectedValueFunctionContext context, HandCombination<TCardGroupName> hand)
    {
        var included = 0;

        foreach (var card in hand.GetCardsInHand(analyzer))
        {
            if (context.EqualityComparer.Equals(card.Category, context.Category))
            {
                included += hand.CountEffectiveCopies(card);
            }
        }

        return included;
    }

    private static double CalculateEffectiveHandSize(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, (IEnumerable<TCategory>, IEqualityComparer<TCategory>) context)
    {
        var (categories, comparer) = context;
        var handSize = 0.0;

        foreach (var hand in analyzer.Combinations)
        {
            var included = 0.0;

            foreach (var category in categories)
            {
                foreach (var card in hand.GetCardsInHand(analyzer))
                {
                    if (comparer.Equals(card.Category, category))
                    {
                        included += hand.CountEffectiveCopies(card);
                    }
                }
            }

            if (included > 0.0)
            {
                handSize += included * analyzer.CalculateProbability(hand);
            }
        }

        return handSize;
    }

    private static double CalculateEffectiveHandSizeWithCardEconomy(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, (IEnumerable<TCategory>, IEqualityComparer<TCategory>) context)
    {
        var (categories, comparer) = context;
        var handSize = 0.0;

        foreach (var hand in analyzer.Combinations)
        {
            var included = 0.0;

            foreach (var category in categories)
            {
                foreach (var card in hand.GetCardsInHand(analyzer))
                {
                    if (comparer.Equals(card.Category, category))
                    {
                        included += hand.CountEffectiveCopies(card) + card.Category.NetCardEconomy;
                    }
                }
            }

            if (included > 0.0)
            {
                handSize += included * analyzer.CalculateProbability(hand);
            }
        }

        return handSize;
    }
}
