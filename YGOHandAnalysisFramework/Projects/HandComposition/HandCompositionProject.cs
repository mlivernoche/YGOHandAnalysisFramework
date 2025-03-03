using YGOHandAnalysisFramework.Data.Extensions;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Configuration;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Projects.HandComposition;

public class HandCompositionProject<TCardGroup, TCardGroupName, TCategory> : IProject<TCardGroup, TCardGroupName>
    where TCardGroup : IHandComposition<TCardGroupName, TCategory>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    where TCategory : notnull, IHandCompositionCategory
{
    protected record ExpectedValueFunctionContext(TCategory Category, IEqualityComparer<TCategory> EqualityComparer);

    private IEnumerable<TCategory> HandCompositionCategories { get; }
    private IEqualityComparer<TCategory> EqualityComparer { get; }

    public HandCompositionProject(
        IEnumerable<TCategory> categories,
        IEqualityComparer<TCategory> equalityComparer)
    {
        HandCompositionCategories = categories;
        EqualityComparer = equalityComparer;
    }

    public string ProjectName { get; } = $"{nameof(HandCompositionProject<TCardGroup, TCardGroupName, TCategory>)}<{typeof(TCardGroup).FullName}, {typeof(TCardGroupName).FullName}, {typeof(TCategory).FullName}>";

    public void Run(ICalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>> calculators, IConfiguration<TCardGroupName> configuration)
    {
        // Not worrying about duplicates
        CompareIncludingDuplicates(calculators)
            .RunInParallel(configuration.FormatterFactory)
            .FormatResults()
            .Write(configuration.OutputStream);

        // Worrying about duplicates
        CompareWithoutIncludingDuplicates(calculators)
            .RunInParallel(configuration.FormatterFactory)
            .FormatResults()
            .Write(configuration.OutputStream);

        // Efficiencies
        {
            var comparison = DataComparison.Create(calculators);

            foreach (var compositionCategory in HandCompositionCategories)
            {
                comparison = comparison.AddCategory(compositionCategory.Name, PercentFormat<double>.Default, new ExpectedValueFunctionContext(compositionCategory, EqualityComparer), static (analyzer, context) =>
                {
                    var ev = analyzer.Calculate(context, static (analyzer, context) => analyzer.CalculateExpectedValue(context, CalculateExpectedValueOfCategory));
                    var effectiveEV = analyzer.Calculate(context, static (analyzer, context) => analyzer.CalculateExpectedValue(context, CalculateEffectiveExpectedValueOfCategory));

                    if (ev == 0.0)
                    {
                        return 0.0;
                    }

                    return effectiveEV / ev;
                },
                compositionCategory.ValueComparer);
            }

            comparison
                .AddCategory("Effective Hand Size", PercentFormat<double>.Default, (HandCompositionCategories, EqualityComparer), static (analyzer, context) =>
                {
                    var handSize = analyzer.Calculate(static analyzer => analyzer.HandSize);
                    var effectiveHandSize = analyzer.Calculate(context, CalculateEffectiveHandSize);

                    if (handSize == 0.0)
                    {
                        return 0.0;
                    }

                    return effectiveHandSize / handSize;
                }, HandAnalyzerComparison.GetDescendingComparer<double>())
                .AddCategory("Net Effective Hand Size", PercentFormat<double>.Default, (HandCompositionCategories, EqualityComparer), static (analyzer, context) =>
                {
                    var handSize = analyzer.Calculate(static analyzer => analyzer.HandSize);
                    var effectiveHandSize = analyzer.Calculate(context, CalculateEffectiveHandSizeWithCardEconomy);

                    if (handSize == 0.0)
                    {
                        return 0.0;
                    }

                    return effectiveHandSize / handSize;
                }, HandAnalyzerComparison.GetDescendingComparer<double>())
                .RunInParallel(configuration.FormatterFactory)
                .FormatResults()
                .Write(configuration.OutputStream);
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
