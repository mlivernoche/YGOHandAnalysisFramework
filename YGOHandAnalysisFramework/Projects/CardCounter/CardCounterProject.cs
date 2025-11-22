using System.Collections.Immutable;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.Configuration;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Extensions.MultipleOK;

namespace YGOHandAnalysisFramework.Projects.CardCounter;

public sealed class CardCounterProject<TCardGroup, TCardGroupName> : IProject<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>, IMultipleOK<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public IReadOnlySet<TCardGroupName> SupportedCardNames { get; }

    private record Context(int CardsToCount, IReadOnlySet<TCardGroupName> CardNames);

    public string ProjectName { get; }

    public CardCounterProject(
        string projectName,
        IEnumerable<TCardGroupName> cardsToCount)
    {
        ProjectName = projectName ?? nameof(CardCounter);
        SupportedCardNames = cardsToCount.ToImmutableHashSet();
    }

    public CardCounterProject(
        string projectName,
        IEnumerable<TCardGroup> cardsToCount)
        : this(projectName, cardsToCount.Select(static group => group.Name))
    { }

    public void Run(ICalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>> calculators, IConfiguration<TCardGroupName> configuration)
    {
        var probabilityFormatter = new PercentFormat<double>();
        var numericalFormatter = new CardinalFormat<double>();

        var maxNumberOfCardsToCount = calculators.Max(static handAnalyzer => handAnalyzer.Calculate(static analyzer => analyzer.HandSize));

        var comparison = DataComparison.Create(calculators);

        for (int i = 0; i <= maxNumberOfCardsToCount; i++)
        {
            comparison = comparison.AddCategory($"Count={i:N0}", probabilityFormatter, analyzer => analyzer.CalculateProbability(hand => CountCards(analyzer, hand) == i));
        }

        for (int i = 0; i <= maxNumberOfCardsToCount; i++)
        {
            comparison = comparison.AddCategory($"Count>={i:N0}", probabilityFormatter, analyzer => analyzer.CalculateProbability(hand => CountCards(analyzer, hand) >= i));
        }

        comparison
            .AddCategory("E(HT)", numericalFormatter, analyzer => analyzer.CalculateExpectedValue((analyzer, hand) => CountCards(analyzer, hand)))
            .RunInParallel(configuration.FormatterFactory)
            .FormatResults()
            .Write(configuration.OutputStream);
    }

    private double CountCards(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, HandCombination<TCardGroupName> hand)
    {
        var total = 0;
        var cardNames = SupportedCardNames;

        foreach (var card in hand.GetCardsInHand(handAnalyzer))
        {
            if (cardNames.Contains(card.Name))
            {
                total += hand.CountEffectiveCopies(card);
            }
        }

        return total;
    }
}
