using CommunityToolkit.Diagnostics;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Data.Operations;

public static class Transform
{
    /// <summary>
    /// Get all the cards in <paramref name="cards"/> with a Size greater than 0, but return the original <typeparamref name="TCardGroup"/>.
    /// This is useful for getting data about the card (e.g., attack, defense, other properties, etc.).
    /// </summary>
    /// <returns>Each <typeparamref name="TCardGroup"/> from <paramref name="analyzer"/> that is present in <paramref name="cards"/>.</returns>
    /// <exception cref="Exception">An exception thrown if a <typeparamref name="TCardGroup"/> is not found in <paramref name="analyzer"/>.</exception>
    public static IEnumerable<TCardGroup> GetCardsInHand<TCardGroup, TCardGroupName>(this HandCombination<TCardGroupName> cards, HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var (card, _) in cards.GetCardsInHand())
        {
            if (!analyzer.CardGroups.TryGetValue(card, out var group))
            {
                throw new Exception($"Card in hand \"{card}\" not in card list.");
            }

            yield return group;
        }
    }

    public delegate TCardGroup CreateMiscCardGroup<TCardGroup, TCardGroupName>(int size, int minSize, int maxSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>;

    public static CardList<CardGroup<TCardGroupName>, TCardGroupName> Optimize<TCardGroupName>(this CardList<CardGroup<TCardGroupName>, TCardGroupName> cardList, IEnumerable<TCardGroupName> cardsToSave, TCardGroupName miscCardGroupName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cardList.Optimize(cardsToSave, (size, min, max) =>
        {
            return new CardGroup<TCardGroupName>()
            {
                Name = miscCardGroupName,
                Size = size,
            };
        });
    }

    public static HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName> Optimize<TCardGroupName>(this HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName> analyzer, IEnumerable<TCardGroupName> cardsToSave, TCardGroupName miscCardGroupName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return analyzer.Optimize(cardsToSave, (size, min, max) =>
        {
            return new CardGroup<TCardGroupName>()
            {
                Name = miscCardGroupName,
                Size = size,
            };
        });
    }

    public static IEnumerable<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>> OptimizeAll<TCardGroupName>(this IEnumerable<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>> handAnalyzers, IEnumerable<TCardGroupName> cardsToSave, TCardGroupName miscCardGroupName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach(var analyzer in handAnalyzers)
        {
            yield return analyzer.Optimize(cardsToSave, miscCardGroupName);
        }
    }

    public static CardList<TCardGroup, TCardGroupName> Optimize<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cardList, IEnumerable<TCardGroupName> cardsToSave, CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardGroups = new HashSet<TCardGroup>();
        var cardsToSaveCopy = cardsToSave is IReadOnlySet<TCardGroupName> names ? names : new HashSet<TCardGroupName>(cardsToSave);

        foreach (var cardGroup in cardList)
        {
            if (cardsToSaveCopy.Contains(cardGroup.Name))
            {
                cardGroups.Add(cardGroup);
            }
        }

        var deckSize = cardList.GetNumberOfCards();
        var cardSize = cardGroups.Sum(static group => group.Size);
        Guard.IsLessThanOrEqualTo(cardSize, deckSize);
        Guard.IsGreaterThan(cardSize, 0);
        var miscSize = deckSize - cardSize;
        var misc = miscFactory(miscSize, 0, miscSize);
        cardGroups.Add(misc);

        return CardList.Create<TCardGroup, TCardGroupName>(cardGroups);
    }

    public static HandAnalyzer<TCardGroup, TCardGroupName> Optimize<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> analyzer, IEnumerable<TCardGroupName> cardsToSave, CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return CardList
            .Create(analyzer)
            .Optimize(cardsToSave, miscFactory)
            .CreateHandAnalyzerBuildArgs(analyzer.AnalyzerName, analyzer.HandSize)
            .CreateHandAnalyzer();
    }

    public static IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> Optimize<TCardGroup, TCardGroupName>(this IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers, IEnumerable<TCardGroupName> cardsToSave, CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var optimizedAnalyzers = new HashSet<HandAnalyzer<TCardGroup, TCardGroupName>>();

        foreach (var analyzer in analyzers)
        {
            optimizedAnalyzers.Add(analyzer.Optimize(cardsToSave, miscFactory));
        }

        return optimizedAnalyzers;
    }

    public static IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> Optimize<TCardGroup, TCardGroupName>(this IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers, Func<HandAnalyzer<TCardGroup, TCardGroupName>, IEnumerable<TCardGroupName>> cardsToSave, CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var optimizedAnalyzers = new HashSet<HandAnalyzer<TCardGroup, TCardGroupName>>();

        foreach (var analyzer in analyzers)
        {
            optimizedAnalyzers.Add(analyzer.Optimize(cardsToSave(analyzer), miscFactory));
        }

        return optimizedAnalyzers;
    }
}
