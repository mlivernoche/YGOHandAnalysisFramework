using YGOHandAnalysisFramework.Data.Extensions;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;

namespace YGOHandAnalysisFramework.Projects.NonEngineCounter;

public class NonEngineCounterProject<TCardGroup, TCardGroupName> : IProject
    where TCardGroup : INonEngineCounterCardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> HandAnalyzers { get; }
    private Transform.CreateMiscCardGroup<TCardGroup, TCardGroupName> MiscFactory { get; }
    private CreateDataComparisonFormat DataComparisonFormatFactory { get; }

    public string ProjectName => nameof(NonEngineCounterProject<TCardGroup, TCardGroupName>);

    public NonEngineCounterProject(
        IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers,
        Transform.CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory,
        CreateDataComparisonFormat createDataComparisonFormat)
    {
        HandAnalyzers = handAnalyzers ?? throw new ArgumentNullException(nameof(handAnalyzers));
        MiscFactory = miscFactory;
        DataComparisonFormatFactory = createDataComparisonFormat;
    }

    public void Run(IHandAnalyzerOutputStream outputStream)
    {
        var probabilityFormatter = new PercentFormat<double>();
        var numericalFormatter = new CardinalFormat<double>();

        var optimizedAnalyzers = HandAnalyzers.Optimize(static analyzer => analyzer
            .CardGroups
            .Values
            .Where(static group => group.IsNonEngine)
            .Select(static group => group.Name)
            .ToHashSet(), MiscFactory);

        var maxNumberOfNonEngine = HandAnalyzers.Max(static handAnalyzer => handAnalyzer.HandSize) + 1;

        var results = DataComparison
            .Create(optimizedAnalyzers)
            .Add(Enumerable.Range(0, maxNumberOfNonEngine).Select(static number =>
            {
                var name = $"HT={number:N0}";
                var format = PercentFormat<double>.Default;
                var context = number;

                var category = new DataComparisonCategory<HandAnalyzer<TCardGroup, TCardGroupName>, int, double>(name, format, context, static (handAnalyzer, num) => handAnalyzer.CalculateProbability(num, HasThisNumberOfNonEngine));
                return category;
            }))
            .Add(Enumerable.Range(0, maxNumberOfNonEngine).Select(static number =>
            {
                var name = $"HT>={number:N0}";
                var format = PercentFormat<double>.Default;
                var context = number;

                var category = new DataComparisonCategory<HandAnalyzer<TCardGroup, TCardGroupName>, int, double>(name, format, context, static (handAnalyzer, num) => handAnalyzer.CalculateProbability(num, HasAtLeastThisNumberOfNonEngine));
                return category;
            }))
            .Add($"E(HT)", numericalFormatter, static handAnalyzer => handAnalyzer.CalculateExpectedValue(static (analyzer, hand) =>
            {
                var total = 0;

                foreach(var card in hand.GetCardsInHand(analyzer))
                {
                    if(card.IsNonEngine)
                    {
                        total += hand.CountEffectiveCopies(card);
                    }
                }

                return total;
            }),
            HandAnalyzerComparison.GetDescendingComparer<double>())
            .Add($"N(HT)", numericalFormatter, static handAnalyzer => handAnalyzer.CardGroups.Values.Where(static group => group.IsNonEngine).Sum(static group => group.Size))
            .RunInParallel(DataComparisonFormatFactory);
        outputStream.Write(results.FormatResults());
    }

    private static bool HasThisNumberOfNonEngine(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, int count, HandCombination<TCardGroupName> hand)
    {
        var total = 0;

        foreach (var card in hand.GetCardsInHand(handAnalyzer))
        {
            if(card.IsNonEngine)
            {
                total += hand.CountEffectiveCopies(card);
            }
        }

        return total == count;
    }

    private static bool HasAtLeastThisNumberOfNonEngine(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, int count, HandCombination<TCardGroupName> hand)
    {
        var total = 0;

        foreach (var card in hand.GetCardsInHand(handAnalyzer))
        {
            if (card.IsNonEngine)
            {
                total += hand.CountEffectiveCopies(card);
            }
        }

        return total >= count;
    }
}
