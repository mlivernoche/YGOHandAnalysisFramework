using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Data.Operations;

public static class Filters
{
    public static bool HasCard<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> analyzer, TCardGroupName cardName)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if(!analyzer.CardGroups.TryGetValue(cardName, out var group))
        {
            return false;
        }

        return group.Size > 0;
    }

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

            if(!analyzer.CardGroups.TryGetValue(element.HandName, out var group))
            {
                throw new Exception($"Card in hand \"{element.HandName}\" not in card list.");
            }

            yield return group;
        }
    }

    private static bool HasCard<TCardGroupName>(HandElement<TCardGroupName> card, IEnumerable<TCardGroupName> contains)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return card.MinimumSize > 0 && contains.Contains(card.HandName);
    }

    public static bool OnlyHasSingles<TCardGroupName>(this HandCombination<TCardGroupName> cards)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards
            .CardNames
            .All(static card => card.MinimumSize <= 1);
    }

    public static bool HasDuplicates<TCardGroupName>(this HandCombination<TCardGroupName> cards)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards
            .CardNames
            .Any(static card => card.MinimumSize > 1);
    }

    public static bool OnlyHasDuplicates<TCardGroupName>(this HandCombination<TCardGroupName> cards)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards
            .CardNames
            .Where(static card => card.MinimumSize > 0)
            .All(static card => card.MinimumSize > 1);
    }

    public static bool HasThisCard<TCardGroupName>(this HandCombination<TCardGroupName> cards, TCardGroupName cardName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var card in cards.GetCardsInHand())
        {
            if (card.HandName.Equals(cardName))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasAnyOfTheseCards<TCardGroupName>(this HandCombination<TCardGroupName> cards, IEnumerable<TCardGroupName> cardNames)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var card in cards.CardNames)
        {
            if (HasCard(card, cardNames))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasAllOfTheseCards<TCardGroupName>(this HandCombination<TCardGroupName> cards, IEnumerable<TCardGroupName> cardNames)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var filtered = cards.GetCardsInHand().Select(static card => card.HandName);
        var set = new HashSet<TCardGroupName>(filtered);
        return set.IsProperSupersetOf(cardNames);
    }

    public static int CountCardNames<TCardGroupName>(this HandCombination<TCardGroupName> cards)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var count = 0;

        foreach (var card in cards.GetCardsInHand())
        {
            count++;
        }

        return count;
    }

    public static int CountCopiesOfCardInHand<TCardGroupName>(this HandCombination<TCardGroupName> cards, TCardGroupName cardName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach(var card in cards.CardNames)
        {
            if(!card.HandName.Equals(cardName))
            {
                continue;
            }

            return card.MinimumSize;
        }

        return 0;
    }

    public static int CountCopiesOfCardInHand<TCardGroupName>(this HandCombination<TCardGroupName> cards, IEnumerable<TCardGroupName> cardNames)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var total = 0;

        foreach(var card in cardNames)
        {
            total += cards.CountCopiesOfCardInHand(card);
        }

        return total;
    }

    public static int CountCardNameInHand<TCardGroupName>(this HandCombination<TCardGroupName> cards, TCardGroupName cardName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {

        foreach (var card in cards.CardNames)
        {
            if (!card.HandName.Equals(cardName))
            {
                continue;
            }

            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Counts each individual card names, but not their duplicates. For example, if there are
    /// two copies of a card, that isn't counted as two, but one.
    /// </summary>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="cards">The hand to analyze.</param>
    /// <param name="cardNames">The cards to look for.</param>
    /// <returns>The amount of names found (this number does not include duplicates).</returns>
    public static int CountCardNamesInHand<TCardGroupName>(this HandCombination<TCardGroupName> cards, IEnumerable<TCardGroupName> cardNames)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var count = 0;

        foreach(var card in cards.CardNames)
        {
            if(!HasCard(card, cardNames))
            {
                continue;
            }

            count++;
        }

        return count;
    }

    public static IEnumerable<TCard> FilterByName<TCard, TCardGroupName, TValue>(this IReadOnlyDictionary<TCardGroupName, TValue> cards, IEnumerable<TCard> names)
        where TCard : INamedCard<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach(var cardName in names)
        {
            if(cards.ContainsKey(cardName.Name))
            {
                yield return cardName;
            }
        }
    }

    public static IEnumerable<T> FilterByName<T, U>(this IReadOnlyDictionary<U, T> cards, IEnumerable<U> names)
    {
        foreach (var cardName in names)
        {
            if (cards.TryGetValue(cardName, out var card))
            {
                yield return card;
            }
        }
    }
}
