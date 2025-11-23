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

    extension<TCardGroupName>(HandCombination<TCardGroupName> hand)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        /// <summary>
        /// Gets a value indicating whether the hand contains only single cards, with no duplicates.
        /// </summary>
        public bool OnlyHasSingles
        {
            get
            {
                var duplicateFound = false;

                foreach (var (_, amount) in hand)
                {
                    if (amount > 1)
                    {
                        duplicateFound = true;
                        break;
                    }
                }

                return duplicateFound;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the hand contains any duplicate items.
        /// </summary>
        public bool HasDuplicates
        {
            get
            {
                foreach (var (_, amount) in hand)
                {
                    if (amount > 1)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether all items in the hand are present only as duplicates, with no single
        /// occurrences.
        /// </summary>
        /// <remarks>Use this property to determine if the hand contains only items that appear more than
        /// once, and no items that appear exactly once. This can be useful for identifying hands composed exclusively
        /// of pairs, triples, or higher multiples.</remarks>
        public bool OnlyHasDuplicates
        {
            get
            {
                var singleFound = false;
                var duplicateFound = false;

                foreach (var (_, amount) in hand)
                {
                    if (amount == 1)
                    {
                        singleFound = true;
                    }
                    else if (amount > 1)
                    {
                        duplicateFound = true;
                    }

                    if (singleFound && duplicateFound)
                    {
                        break;
                    }
                }

                return !singleFound && duplicateFound;
            }
        }

        /// <summary>
        /// Determines whether the hand contains at least one card of the specified card group.
        /// </summary>
        /// <param name="cardName">The name of the card group to check for in the hand.</param>
        /// <returns>true if the hand contains one or more cards of the specified group; otherwise, false.</returns>
        public bool HasThisCard(TCardGroupName cardName)
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
        /// Determines whether the hand contains at least one card with a name from the specified collection.
        /// </summary>
        /// <param name="cardNames">A collection of card group names to check for in the hand. Cannot be null.</param>
        /// <returns>true if the hand contains at least one card whose name is in the specified collection; otherwise, false.</returns>
        public bool HasAnyOfTheseCards(IEnumerable<TCardGroupName> cardNames)
        {
            IReadOnlySet<TCardGroupName> cardNamesSet = cardNames is IReadOnlySet<TCardGroupName> set ? set : cardNames.ToHashSet();

            foreach (var (name, amount) in hand)
            {
                if(amount == 0)
                {
                    continue;
                }

                if (cardNamesSet.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the hand contains at least one card with a name matching any of the specified card names.
        /// </summary>
        /// <param name="cardNames">A set of card names to search for in the hand. Each element represents a card name to match. The method
        /// returns true if any card in the hand has a name equal to one of these values.</param>
        /// <returns>true if the hand contains at least one card with a name matching any of the specified card names; otherwise,
        /// false.</returns>
        public bool HasAnyOfTheseCards(params ReadOnlySpan<TCardGroupName> cardNames)
        {
            foreach (var (name, amount) in hand)
            {
                if(amount == 0)
                {
                    continue;
                }

                foreach (var cardSearch in cardNames)
                {
                    if (cardSearch.Equals(name))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the hand contains all of the specified card group names.
        /// </summary>
        /// <param name="cardNames">A collection of card group names to check for presence in the hand. Cannot be null.</param>
        /// <returns>true if the hand contains every card group name in the specified collection; otherwise, false.</returns>
        public bool HasAllOfTheseCards(IEnumerable<TCardGroupName> cardNames)
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
        /// Gets the number of distinct card names in the hand that have a nonzero amount.
        /// </summary>
        public int NumberOfCardNames
        {
            get
            {
                var count = 0;

                foreach (var (_, amount) in hand)
                {
                    if (amount == 0)
                    {
                        continue;
                    }

                    count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Returns the number of copies of the specified card currently in the hand.
        /// </summary>
        /// <param name="cardName">The name of the card to count. Must not be null.</param>
        /// <returns>The number of copies of the specified card in the hand. Returns 0 if the card is not present.</returns>
        public int CountCopiesOfCardInHand(TCardGroupName cardName)
        {
            foreach (var (name, amount) in hand)
            {
                if(amount == 0)
                {
                    continue;
                }

                if (!name.Equals(cardName))
                {
                    continue;
                }

                return amount;
            }

            return 0;
        }

        /// <summary>
        /// Calculates the total number of cards in the hand that match any of the specified card names.
        /// </summary>
        /// <param name="cardNames">A collection of card names to count in the hand. Each name in the collection is counted separately. Cannot
        /// be null.</param>
        /// <returns>The total number of cards in the hand that match any of the specified card names. Returns 0 if none of the
        /// specified cards are present.</returns>
        public int CountCopiesOfCardInHand(IEnumerable<TCardGroupName> cardNames)
        {
            var total = 0;

            foreach (var card in cardNames)
            {
                total += hand.CountCopiesOfCardInHand(card);
            }

            return total;
        }

        /// <summary>
        /// Counts the total number of cards in the hand that match any of the specified card names.
        /// </summary>
        /// <param name="cardNames">A parameter array of card names to search for in the hand. Each element specifies a card name to count.
        /// Cannot be empty.</param>
        /// <returns>The total number of cards in the hand that match any of the specified card names. Returns 0 if none of the
        /// specified card names are present.</returns>
        public int CountCopiesOfCardInHand(params ReadOnlySpan<TCardGroupName> cardNames)
        {
            var total = 0;

            foreach (var card in cardNames)
            {
                total += hand.CountCopiesOfCardInHand(card);
            }

            return total;
        }
    }

    /// <summary>
    /// Filters a sequence of cards, returning only those whose names exist as keys in the specified dictionary.
    /// </summary>
    /// <remarks>The order of the returned cards matches the order of the input sequence. No values from the
    /// dictionary are used; only the presence of the key is checked.</remarks>
    /// <typeparam name="TCard">The type of card to filter. Must implement INamedCard&lt;TCardGroupName&gt;.</typeparam>
    /// <typeparam name="TCardGroupName">The type used as the key for card names. Must be non-null, equatable, and comparable.</typeparam>
    /// <typeparam name="TValue">The type of values stored in the dictionary for each card group name.</typeparam>
    /// <param name="cards">A read-only dictionary containing card group names as keys. Only cards whose names are present as keys in this
    /// dictionary will be included in the result.</param>
    /// <param name="names">A sequence of cards to filter based on their names.</param>
    /// <returns>An enumerable collection of cards from the input sequence whose names are present as keys in the dictionary.</returns>
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

    /// <summary>
    /// Returns a sequence of values from the dictionary whose keys match the specified names.
    /// </summary>
    /// <typeparam name="T">The type of the values in the dictionary.</typeparam>
    /// <typeparam name="U">The type of the keys used to identify values in the dictionary.</typeparam>
    /// <param name="cards">The dictionary containing values to filter, keyed by name.</param>
    /// <param name="names">The collection of keys to match against the dictionary.</param>
    /// <returns>An enumerable collection of values from the dictionary whose keys are present in <paramref name="names"/>. The
    /// order of the returned values corresponds to the order of the keys in <paramref name="names"/>. If a key is not
    /// found in the dictionary, it is skipped.</returns>
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
