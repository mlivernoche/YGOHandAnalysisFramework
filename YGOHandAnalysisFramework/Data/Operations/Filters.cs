using CommunityToolkit.Diagnostics;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Data.Operations;

public static class Filters
{
    /// <summary>
    /// Checks whether <paramref name="analyzer" /> contains a card with the name <paramref name="cardName" /> and whose Size is greater than 0.
    /// </summary>
    /// <returns>True is <paramref name="cardName" /> is found in <paramref name="analyzer" /> and its Size is greater than 0, otherwise false.</returns>
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

    /// <summary>
    /// Check whether <paramref name="hand"/> is only contained of unique <typeparamref name="TCardGroupName"/> card names (all no singles, no duplicates).
    /// </summary>
    /// <returns>True if <paramref name="hand"/> only has unique cards, otherwise false.</returns>
    public static bool OnlyHasSingles<TCardGroupName>(this HandCombination<TCardGroupName> hand)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var duplicateFound = false;

        foreach(var (_, amount) in hand)
        {
            if(amount > 1)
            {
                duplicateFound = true;
                break;
            }
        }

        return duplicateFound;
    }

    /// <summary>
    /// Checks whether <paramref name="hand"/> has at least one set of duplicate <typeparamref name="TCardGroupName"/> card names (can be a pair or a trio).
    /// </summary>
    /// <returns>True if <paramref name="hand"/> has at least one set of duplicate <typeparamref name="TCardGroupName"/> card names, otherwise false.</returns>
    public static bool HasDuplicates<TCardGroupName>(this HandCombination<TCardGroupName> hand)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach(var (_, amount) in hand)
        {
            if(amount > 1)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether <paramref name="cards"/> is ONLY composed of duplicate <typeparamref name="TCardGroupName"/> card names (these can be pairs, but also trios).
    /// e.g. a hand like 3x "Card A" and 2x "Card B" would return TRUE.
    /// </summary>
    /// <returns>True if <paramref name="cards"/> is ONLY composted of duplicate <typeparamref name="TCardGroupName"/> card names, otherwise false.</returns>
    public static bool OnlyHasDuplicates<TCardGroupName>(this HandCombination<TCardGroupName> hand)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var singleFound = false;
        var duplicateFound = false;

        foreach(var (_, amount) in hand)
        {
            if(amount == 1)
            {
                singleFound = true;
            }
            else if(amount > 1)
            {
                duplicateFound = true;
            }

            if(singleFound && duplicateFound)
            {
                break;
            }
        }

        return !singleFound && duplicateFound;
    }

    /// <summary>
    /// Check whether <paramref name="cardName"/> is present in the <paramref name="hand"/>.
    /// </summary>
    /// <returns>True if <paramref name="cardName"/> is in <paramref name="hand"/>, otherwise false.</returns>
    public static bool HasThisCard<TCardGroupName>(this HandCombination<TCardGroupName> hand, TCardGroupName cardName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var (name, amount) in hand)
        {
            if (name.Equals(cardName))
            {
                return amount > 0;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether ANY of the card names in <paramref name="cardNames"/> are present in <paramref name="hand"/>.
    /// </summary>
    /// <returns>True if ANY of the card names in <paramref name="cardNames"/> are present in <paramref name="hand"/>, otherwise false.</returns>
    public static bool HasAnyOfTheseCards<TCardGroupName>(this HandCombination<TCardGroupName> hand, IEnumerable<TCardGroupName> cardNames)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        IReadOnlySet<TCardGroupName> cardNamesSet = cardNames is IReadOnlySet<TCardGroupName> set ? set : cardNames.ToHashSet();

        foreach (var (name, _) in hand.GetCardsInHand())
        {
            if (cardNamesSet.Contains(name))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether ANY of the card names in <paramref name="cardNames"/> are present in <paramref name="hand"/>.
    /// </summary>
    /// <remarks>This method uses <c>params ReadOnlySpan&lt;&gt;</c>, <see href="https://devblogs.microsoft.com/dotnet/csharp13-calling-methods-is-easier-and-faster/#params-collections">which can be better optimized by .NET</see>.</remarks>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="hand">The hand being checked for cards in <paramref name="cardNames"/>.</param>
    /// <param name="cardNames">The card names being checked in <paramref name="hand"/>.</param>
    /// <returns>True if ANY of the cards in <paramref name="cardNames"/> are in present in <paramref name="hand"/>, otherwise false.</returns>
    public static bool HasAnyOfTheseCards<TCardGroupName>(this HandCombination<TCardGroupName> hand, params ReadOnlySpan<TCardGroupName> cardNames)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach(var (name, _) in hand.GetCardsInHand())
        {
            foreach(var cardSearch in cardNames)
            {
                if(cardSearch.Equals(name))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether ALL of the card names in <paramref name="cardNames"/> are present in <paramref name="hand"/>.
    /// </summary>
    /// <returns>True if ALL of the card names in <paramref name="cardNames"/> are present in <paramref name="hand"/>, otherwise false.</returns>
    public static bool HasAllOfTheseCards<TCardGroupName>(this HandCombination<TCardGroupName> hand, IEnumerable<TCardGroupName> cardNames)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var included in cardNames)
        {
            if (!hand.HasThisCard(included))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Counts the amount of unique card names in <paramref name="hand"/>
    /// </summary>
    /// <returns>The amount of unique card names in <paramref name="hand"/>.</returns>
    public static int CountCardNames<TCardGroupName>(this HandCombination<TCardGroupName> hand)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var count = 0;

        foreach (var card in hand.GetCardsInHand())
        {
            count++;
        }

        return count;
    }

    /// <summary>
    /// Counts the amount of copies of <paramref name="cardName"/> in <paramref name="hand"/>.
    /// </summary>
    /// <returns>The amount of copies of <paramref name="cardName"/> in <paramref name="hand"/>.</returns>
    public static int CountCopiesOfCardInHand<TCardGroupName>(this HandCombination<TCardGroupName> hand, TCardGroupName cardName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach(var (name, amount) in hand.GetCardsInHand())
        {
            if(!name.Equals(cardName))
            {
                continue;
            }

            return amount;
        }

        return 0;
    }

    /// <summary>
    /// Counts the total amount of all card in <paramref name="cardNames"/> present in <paramref name="hand"/>.
    /// </summary>
    /// <returns>The total amount of all card in <paramref name="cardNames"/> present in <paramref name="hand"/>.</returns>
    public static int CountCopiesOfCardInHand<TCardGroupName>(this HandCombination<TCardGroupName> hand, IEnumerable<TCardGroupName> cardNames)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var total = 0;

        foreach(var card in cardNames)
        {
            total += hand.CountCopiesOfCardInHand(card);
        }

        return total;
    }

    /// <summary>
    /// Counts the total amount of all card in <paramref name="cardNames"/> present in <paramref name="hand"/>.
    /// </summary>
    /// <remarks>This method uses <c>params ReadOnlySpan&lt;&gt;</c>, <see href="https://devblogs.microsoft.com/dotnet/csharp13-calling-methods-is-easier-and-faster/#params-collections">which can be better optimized by .NET</see>.</remarks>
    /// <returns>The total amount of all card in <paramref name="cardNames"/> present in <paramref name="hand"/>.</returns>
    public static int CountCopiesOfCardInHand<TCardGroupName>(this HandCombination<TCardGroupName> hand, params ReadOnlySpan<TCardGroupName> cardNames)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var total = 0;

        foreach (var card in cardNames)
        {
            total += hand.CountCopiesOfCardInHand(card);
        }

        return total;
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
