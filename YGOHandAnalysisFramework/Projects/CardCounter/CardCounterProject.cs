using System.Collections.Immutable;
using YGOHandAnalysisFramework.Data.Extensions;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Projects.CardCounter;

public sealed class CardCounterProject<TCardGroup, TCardGroupName> : IProject
    where TCardGroup : IMultipleOK<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> HandAnalyzers { get; }
    private IReadOnlySet<TCardGroupName> CardsToCount { get; }
    private Transform.CreateMiscCardGroup<TCardGroup, TCardGroupName> MiscFactory { get; }
    private CreateDataComparisonFormatter DataComparisonFormatFactory { get; }

    private record Context(int CardsToCount, IReadOnlySet<TCardGroupName> CardNames);

    public string ProjectName { get; }

    public CardCounterProject(
        string projectName,
        IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers,
        IEnumerable<TCardGroupName> cardsToCount,
        Transform.CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory,
        CreateDataComparisonFormatter dataComparisonFormatFactory)
    {
        ProjectName = projectName ?? nameof(CardCounter);
        HandAnalyzers = handAnalyzers ?? throw new ArgumentNullException(nameof(handAnalyzers));
        CardsToCount = cardsToCount.ToImmutableHashSet();
        MiscFactory = miscFactory ?? throw new ArgumentNullException(nameof(miscFactory));
        DataComparisonFormatFactory = dataComparisonFormatFactory ?? throw new ArgumentNullException(nameof(dataComparisonFormatFactory));
    }

    public CardCounterProject(
        string projectName,
        IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers,
        IEnumerable<TCardGroup> cardsToCount,
        Transform.CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory,
        CreateDataComparisonFormatter dataComparisonFormatFactory)
        : this(
              projectName,
              handAnalyzers,
              cardsToCount.Select(static group => group.Name),
              miscFactory,
              dataComparisonFormatFactory)
    { }

    public void Run(IHandAnalyzerOutputStream outputStream)
    {
        var probabilityFormatter = new PercentFormat<double>();
        var numericalFormatter = new CardinalFormat<double>();

        var optimizedAnalyzers = HandAnalyzers.Optimize(CardsToCount, MiscFactory);

        var maxNumberOfCardsToCount = optimizedAnalyzers.Max(static handAnalyzer => handAnalyzer.HandSize);

        var comparison = DataComparison.Create(optimizedAnalyzers);

        for (int i = 0; i <= maxNumberOfCardsToCount; i++)
        {
            comparison = comparison.AddCategory($"Count={i:N0}", probabilityFormatter, new Context(i, CardsToCount), static (handAnalyzer, context) => handAnalyzer.CalculateProbability(context, HasThisNumberOfCards));
        }

        for (int i = 0; i <= maxNumberOfCardsToCount; i++)
        {
            comparison = comparison.AddCategory($"Count>={i:N0}", probabilityFormatter, new Context(i, CardsToCount), static (handAnalyzer, num) => handAnalyzer.CalculateProbability(num, HasAtLeastThisNumberOfCards));
        }

        var results = comparison
            .AddCategory("E(HT)", numericalFormatter, new Context(0, CardsToCount), static (handAnalyzer, context) => handAnalyzer.CalculateExpectedValue(context, CountCards))
            .RunInParallel(DataComparisonFormatFactory);
        outputStream.Write(results.FormatResults());
    }

    private static double CountCards(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context context, HandCombination<TCardGroupName> hand)
    {
        var total = 0;
        var cardNames = context.CardNames;

        foreach (var card in hand.GetCardsInHand(handAnalyzer))
        {
            if (cardNames.Contains(card.Name))
            {
                total += hand.CountEffectiveCopies(card);
            }
        }

        return total;
    }

    private static bool HasThisNumberOfCards(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context context, HandCombination<TCardGroupName> hand)
    {
        return CountCards(handAnalyzer, context, hand) == context.CardsToCount;
    }

    private static bool HasAtLeastThisNumberOfCards(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context context, HandCombination<TCardGroupName> hand)
    {
        return CountCards(handAnalyzer, context, hand) >= context.CardsToCount;
    }
}
