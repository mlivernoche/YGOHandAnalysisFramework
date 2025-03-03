using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Extensions.Linq;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.WeightedProbability;

namespace YGOHandAnalysisFramework.Features.Configuration;

public static class Configuration
{
    public static bool AreAllCardNamesRecognized<TCardGroupName>(this IConfiguration<TCardGroupName> config, IReadOnlySet<TCardGroupName> allCardNames, [NotNullWhen(false)] out IEnumerable<TCardGroupName>? cardsNotFound)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var namesMentioned = config
            .DeckLists
            .SelectMany(static deckList => deckList.Cards.Select(static group => group.Name))
            .ToHashSet();
        var cardsNotFoundUnique = new HashSet<TCardGroupName>();

        foreach(var mentioned in namesMentioned)
        {
            if(!allCardNames.Contains(mentioned))
            {
                cardsNotFoundUnique.Add(mentioned);
            }
        }

        if(cardsNotFoundUnique.Count != 0)
        {
            cardsNotFound = cardsNotFoundUnique;
            return false;
        }

        cardsNotFound = default;
        return true;
    }

    private static ICardGroupCollection<TCardGroup, TCardGroupName> Filter<TCardGroup, TCardGroupName>(ICardGroupCollection<TCardGroup, TCardGroupName> original, IReadOnlySet<TCardGroupName> supportedCards)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if(supportedCards.Count == 0)
        {
            return original;
        }

        var cardsThatAreSupported = new CardGroupCollection<TCardGroup, TCardGroupName>();

        foreach (var card in original)
        {
            if (!supportedCards.Contains(card.Name))
            {
                continue;
            }

            cardsThatAreSupported = cardsThatAreSupported.Add(card);
        }

        return cardsThatAreSupported;
    }

    public static IReadOnlyCollection<ICalculatorWrapper<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>>> CreateAnalyzers<TCardGroupName>(this IConfiguration<TCardGroupName> config, TCardGroupName miscCardGroupName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return config.CreateAnalyzers(size => CardGroup.Create(miscCardGroupName, size, 0, size), static cardGroup => cardGroup, ImmutableHashSet<TCardGroupName>.Empty);
    }

    public static IReadOnlyCollection<ICalculatorWrapper<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>>> CreateAnalyzers<TCardGroupName>(this IConfiguration<TCardGroupName> config, TCardGroupName miscCardGroupName, IReadOnlySet<TCardGroupName> supportedCards)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return config.CreateAnalyzers(size => CardGroup.Create(miscCardGroupName, size, 0, size), static cardGroup => cardGroup, supportedCards);
    }

    public static IReadOnlyCollection<ICalculatorWrapper<HandAnalyzer<TCardGroup, TCardGroupName>>> CreateAnalyzers<TCardGroup, TCardGroupName>(this IConfiguration<TCardGroupName> config, Func<int, TCardGroup> miscGroupFactory, Func<CardGroup<TCardGroupName>, TCardGroup> cardGroupFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return config.CreateAnalyzers(miscGroupFactory, cardGroupFactory, ImmutableHashSet<TCardGroupName>.Empty);
    }

    public static ICalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>> CreateAnalyzers<TCardGroup, TCardGroupName>(this IConfiguration<TCardGroupName> config, Func<int, TCardGroup> miscGroupFactory, Func<CardGroup<TCardGroupName>, TCardGroup> cardGroupFactory, IReadOnlySet<TCardGroupName> supportedCards)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var analyzersCollection = new CalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>>();

        foreach (var deckList in config.DeckLists)
        {
            var cardsThatAreSupported = Filter(deckList.Cards, supportedCards);
            var fillSize = Math.Max(deckList.Cards.GetNumberOfCards(), config.CardListFillSize);

            var cardList = CardList
                .Create<TCardGroup, TCardGroupName>(cardsThatAreSupported.Select(cardGroupFactory))
                .Fill(fillSize, size => miscGroupFactory(size));

            var buildArgs = config
                .HandSizes
                .Select(handSize => HandAnalyzerBuildArguments.Create($"{deckList.Name}, {handSize:N0}", handSize, cardList));

            var handAnalyzers = HandAnalyzer.CreateInParallel(buildArgs);
            foreach (var analyzer in handAnalyzers.OrderBy(buildArgs))
            {
                analyzersCollection.Add(analyzer);
            }

            if(config.CreateWeightedProbabilities)
            {
                var weightedProbabilities = WeightedProbabilityCollection.CreateWithEqualWeights(deckList.Name, handAnalyzers.OrderBy(buildArgs));
                analyzersCollection.Add(weightedProbabilities);
            }
        }

        return analyzersCollection;
    }
}
