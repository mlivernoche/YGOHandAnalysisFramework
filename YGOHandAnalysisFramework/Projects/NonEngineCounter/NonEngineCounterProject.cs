using YGOHandAnalysisFramework.Data.Extensions;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Configuration;

namespace YGOHandAnalysisFramework.Projects.NonEngineCounter;

public class NonEngineCounterProject<TCardGroup, TCardGroupName> : IProject<TCardGroup, TCardGroupName>
    where TCardGroup : INonEngineCounterCardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public string ProjectName => nameof(NonEngineCounterProject<TCardGroup, TCardGroupName>);

    public void Run(ICalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>> calculators, IConfiguration<TCardGroupName> configuration)
    {
        var probabilityFormatter = new PercentFormat<double>();
        var numericalFormatter = new CardinalFormat<double>();

        var maxNumberOfNonEngine = Convert.ToInt32(calculators.Max(static handAnalyzer => handAnalyzer.Calculate(static analyzer => analyzer.HandSize))) + 1;

        DataComparison
            .Create(calculators)
            .AddCategories(Enumerable.Range(0, maxNumberOfNonEngine).Select(static number =>
            {
                var name = $"HT={number:N0}";
                var format = PercentFormat<double>.Default;

                var category = new DataComparisonCategory<ICalculatorWrapper<HandAnalyzer<TCardGroup, TCardGroupName>>, double>(name, format, HasThisNumberOfNonEngine(number).Wrap());
                return category;
            }))
            .AddCategories(Enumerable.Range(0, maxNumberOfNonEngine).Select(static number =>
            {
                var name = $"HT>={number:N0}";
                var format = PercentFormat<double>.Default;
                var context = number;

                var category = new DataComparisonCategory<ICalculatorWrapper<HandAnalyzer<TCardGroup, TCardGroupName>>, double>(name, format, HasAtLeastThisNumberOfNonEngine(number).Wrap());
                return category;
            }))
            .AddCategory($"E(HT)", numericalFormatter, static analyzer => analyzer.CalculateExpectedValue(static (analyzer, hand) =>
            {
                var total = 0.0;

                foreach (var card in hand.GetCardsInHand(analyzer))
                {
                    if (card.IsNonEngine)
                    {
                        total += hand.CountEffectiveCopies(card);
                    }
                }

                return total;
            }), HandAnalyzerComparison.GetDescendingComparer<double>())
            .AddCategory($"N(HT)", numericalFormatter, static analyzer => analyzer.CardGroups.Values.Where(static group => group.IsNonEngine).Sum(static group => group.Size))
            .RunInParallel(configuration.FormatterFactory)
            .FormatResults()
            .Write(configuration.OutputStream);
    }

    private static Func<HandAnalyzer<TCardGroup, TCardGroupName>, double> HasThisNumberOfNonEngine(int numberToCount)
    {
        return analyzer => analyzer.CalculateProbability((analyzer, hand) =>
        {
            var total = 0;

            foreach (var card in hand.GetCardsInHand(analyzer))
            {
                if (card.IsNonEngine)
                {
                    total += hand.CountEffectiveCopies(card);
                }
            }

            return total == numberToCount;
        });
    }

    private static Func<HandAnalyzer<TCardGroup, TCardGroupName>, double> HasAtLeastThisNumberOfNonEngine(int numberToCount)
    {
        return analyzer => analyzer.CalculateProbability((analyzer, hand) =>
        {
            var total = 0;

            foreach (var card in hand.GetCardsInHand(analyzer))
            {
                if (card.IsNonEngine)
                {
                    total += hand.CountEffectiveCopies(card);
                }
            }

            return total >= numberToCount;
        });
    }
}
