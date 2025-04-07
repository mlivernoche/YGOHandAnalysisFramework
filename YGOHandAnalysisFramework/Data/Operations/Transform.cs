using CommunityToolkit.Diagnostics;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Data.Operations;

public static class Transform
{

    /// <summary>
    /// Get all the cards in <paramref name="cards"/> with a Size greater than 0.
    /// </summary>
    /// <returns>All cards present in <paramref name="cards"/> with a Size greater than 0.</returns>
    public static IEnumerable<HandElement<TCardGroupName>> GetCardsInHand<TCardGroupName>(this HandCombination<TCardGroupName> cards)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var element in cards.CardNames)
        {
            if (element.MinimumSize == 0)
            {
                continue;
            }

            yield return element;
        }
    }

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
        foreach (var element in cards.CardNames)
        {
            if (element.MinimumSize == 0)
            {
                continue;
            }

            if (!analyzer.CardGroups.TryGetValue(element.HandName, out var group))
            {
                throw new Exception($"Card in hand \"{element.HandName}\" not in card list.");
            }

            yield return group;
        }
    }

    public static IEnumerable<(TCardGroup, HandElement<TCardGroupName>)> GetCardGroups<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> analyzer, HandCombination<TCardGroupName> hand)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var card in hand.GetCardsInHand())
        {
            if (analyzer.CardGroups.TryGetValue(card.HandName, out var cardGroup))
            {
                yield return (cardGroup, card);
            }
        }
    }

    public delegate TCardGroup CreateMiscCardGroup<TCardGroup, TCardGroupName>(int size, int minSize, int maxSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>;

    public static HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName> Optimize<TCardGroupName>(this HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName> analyzer, IEnumerable<TCardGroupName> cardsToSave, TCardGroupName miscCardGroupName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return analyzer.Optimize(cardsToSave, (size, min, max) =>
        {
            return new CardGroup<TCardGroupName>()
            {
                Name = miscCardGroupName,
                Size = size,
                Minimum = min,
                Maximum = max,
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

    public static HandAnalyzer<TCardGroup, TCardGroupName> Optimize<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> analyzer, IEnumerable<TCardGroupName> cardsToSave, CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardGroups = new HashSet<TCardGroup>();
        var cardsToSaveCopy = cardsToSave is IReadOnlySet<TCardGroupName> names ? names : new HashSet<TCardGroupName>(cardsToSave);

        foreach (var (cardGroupName, cardGroup) in analyzer.CardGroups)
        {
            if (cardsToSaveCopy.Contains(cardGroupName))
            {
                cardGroups.Add(cardGroup);
            }
        }

        var cardSize = cardGroups.Sum(static group => group.Size);
        Guard.IsLessThanOrEqualTo(cardSize, analyzer.DeckSize);
        Guard.IsGreaterThan(cardSize, 0);
        var miscSize = analyzer.DeckSize - cardSize;
        var misc = miscFactory(miscSize, 0, miscSize);
        cardGroups.Add(misc);

        var cardList = CardList.Create<TCardGroup, TCardGroupName>(cardGroups);
        var analyzerArgs = HandAnalyzerBuildArguments.Create(analyzer.AnalyzerName, analyzer.HandSize, cardList);
        return HandAnalyzer.Create(analyzerArgs);
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

    public static HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName> ConvertToCardGroup<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardList = CardList
            .Create(handAnalyzer)
            .Cast(static group => new CardGroup<TCardGroupName>()
            {
                Name = group.Name,
                Size = group.Size,
                Minimum = group.Minimum,
                Maximum = group.Maximum,
            });
        var buildArgs = HandAnalyzerBuildArguments.Create(handAnalyzer.AnalyzerName, handAnalyzer.HandSize, cardList);
        return new HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>(buildArgs, handAnalyzer.Combinations);
    }


    public static IEnumerable<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>> ConvertToCardGroup<TCardGroup, TCardGroupName>(this IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var handAnalyzer in handAnalyzers)
        {
            yield return handAnalyzer.ConvertToCardGroup();
        }
    }
}
