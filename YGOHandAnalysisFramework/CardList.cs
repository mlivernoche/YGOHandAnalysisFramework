using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using CommunityToolkit.Diagnostics;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework;

public static class CardList
{
    /// <summary>
    /// Create an empty <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list.
    /// </summary>
    /// <typeparam name="TCardGroup">The type to use for the card groups. Card groups are a bundle of cards with the same name (identified by <typeparamref name="TCardGroupName"/>). It can also have addition data, such as the card's ATK, DEF, etc.</typeparam>
    /// <typeparam name="TCardGroupName">The type of the name used to identify the card of each card group.</typeparam>
    /// <returns>The empty <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list.</returns>
    [Pure]
    public static CardList<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>()
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardList<TCardGroup, TCardGroupName>([]);
    }

    /// <summary>
    /// Create a <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list from a <c>ICardGroupCollection&lt;<typeparamref name="TCardGroup" />, <typeparamref name="TCardGroupName" />&gt;</c> <paramref name="cardGroups"/>.
    /// </summary>
    /// <typeparam name="TCardGroup">The type to use for the card groups. Card groups are a bundle of cards with the same name (identified by <typeparamref name="TCardGroupName"/>). It can also have addition data, such as the card's ATK, DEF, etc.</typeparam>
    /// <typeparam name="TCardGroupName">The type of the name used to identify the card of each card group.</typeparam>
    /// <param name="cardGroups">The card groups to add to the <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list.</param>
    /// <returns>The <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list with all the card groups.</returns>
    [Pure]
    public static CardList<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(ICardGroupCollection<TCardGroup, TCardGroupName> cardGroups)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardList<TCardGroup, TCardGroupName>(cardGroups);
    }
    /// <summary>
    /// Create a <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list from a <c>IEnumerable&lt;<typeparamref name="TCardGroup" />&gt;</c> <paramref name="cardGroups"/>.
    /// </summary>
    /// <typeparam name="TCardGroup">The type to use for the card groups. Card groups are a bundle of cards with the same name (identified by <typeparamref name="TCardGroupName"/>). It can also have addition data, such as the card's ATK, DEF, etc.</typeparam>
    /// <typeparam name="TCardGroupName">The type of the name used to identify the card of each card group.</typeparam>
    /// <param name="cardGroups">The card groups to add to the <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list.</param>
    /// <returns>The <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list with all the card groups.</returns>
    [Pure]
    public static CardList<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(IEnumerable<TCardGroup> cardGroups)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardList<TCardGroup, TCardGroupName>(cardGroups);
    }

    /// <summary>
    /// Create a <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list from a <c>HandAnalyzer&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> <paramref name="analyzer"/>.
    /// </summary>
    /// <typeparam name="TCardGroup">The type to use for the card groups. Card groups are a bundle of cards with the same name (identified by <typeparamref name="TCardGroupName"/>). It can also have addition data, such as the card's ATK, DEF, etc.</typeparam>
    /// <typeparam name="TCardGroupName">The type of the name used to identify the card of each card group.</typeparam>
    /// <param name="analyzer">The analyzer which will be used to build the <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list.</param>
    /// <returns>The <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list with all the card groups.</returns>
    [Pure]
    public static CardList<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardList<TCardGroup, TCardGroupName>(analyzer.CardGroups.Values);
    }

    /// <summary>
    /// An assertion. If the provided <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> <paramref name="cardList"/> is not equal
    /// to the provided <c>size</c>, then an exception will be thrown.
    /// </summary>
    /// <typeparam name="TCardGroup">The type to use for the card groups. Card groups are a bundle of cards with the same name (identified by <typeparamref name="TCardGroupName"/>). It can also have addition data, such as the card's ATK, DEF, etc.</typeparam>
    /// <typeparam name="TCardGroupName">The type of the name used to identify the card of each card group.</typeparam>
    /// <param name="cardList">the provided <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> card list</param>
    /// <param name="expectedDeckListSize">The expected size of the <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> <paramref name="cardList"/>. If they differ, then an exception is thrown.</param>
    /// <returns>The provided <c>CardList&lt;<typeparamref name="TCardGroup"/>, <typeparamref name="TCardGroupName"/>&gt;</c> <paramref name="cardList"/>.</returns>
    [Pure]
    public static CardList<TCardGroup, TCardGroupName> AssertSize<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cardList, int expectedDeckListSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var listSize = cardList.GetNumberOfCards();
        Guard.IsEqualTo(expectedDeckListSize, listSize);
        return cardList;
    }

    [Pure]
    public static CardList<TCardGroup, TCardGroupName> Change<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cards, TCardGroup card)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards.Change(card);
    }

    [Pure]
    public static CardList<TCardGroup, TCardGroupName> Change<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cards, TCardGroupName name, Func<TCardGroup, TCardGroup> change)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards.Change(name, change);
    }

    [Pure]
    public static CardList<TCardGroup, TCardGroupName> Remove<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cards, TCardGroupName card)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cards.Remove(card);
    }

    [Pure]
    public static CardList<TCardGroup, TCardGroupName> RemoveHand<TCardGroup, TCardGroupName>(this IEnumerable<TCardGroup> cards, HandCombination<TCardGroupName> hand)
        where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var dict = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, cards);

        foreach (var cardInHand in hand.CardNames)
        {
            Guard.IsTrue(dict.TryGetValue(cardInHand.HandName, out var card));

            var newAmount = card.Size - cardInHand.MinimumSize;
            if (newAmount == 0)
            {
                dict.TryRemove(card);
            }
            else
            {
                var newCard = card.ChangeSize(newAmount);

                if(typeof(TCardGroup).IsClass)
                {
                    Guard.IsFalse(ReferenceEquals(card, newCard));
                }

                dict.AddOrUpdate(newCard);
            }
        }

        return new CardList<TCardGroup, TCardGroupName>(dict.Values);
    }

    [Pure]
    public static CardList<TCardGroup, TCardGroupName> RemoveCard<TCardGroup, TCardGroupName>(this IEnumerable<TCardGroup> cards, TCardGroupName cardInHand)
        where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var dict = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, cards);

        if (dict.TryGetValue(cardInHand, out var card))
        {
            var newAmount = card.Size - 1;
            if (newAmount == 0)
            {
                dict.TryRemove(card);
            }
            else
            {
                var newCard = card.ChangeSize(newAmount);

                if (typeof(TCardGroup).IsClass)
                {
                    Guard.IsFalse(ReferenceEquals(card, newCard));
                }

                dict.AddOrUpdate(newCard);
            }
        }

        return new CardList<TCardGroup, TCardGroupName>(dict.Values);
    }

    /// <summary>
    /// Gets the deck size of the card list; the summation of all .Size in the card list.
    /// </summary>
    /// <param name="cardList">The card list.</param>
    /// <returns>The deck size.</returns>
    [Pure]
    public static int GetNumberOfCards<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cardList)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cardList.Sum(static group => group.Size);
    }

    /// <summary>
    /// Gets the deck size of the card list; the summation of all .Size in the card list.
    /// </summary>
    /// <param name="cardList">The card list.</param>
    /// <returns>The deck size.</returns>
    [Pure]
    public static int GetNumberOfCards<TCardGroup, TCardGroupName>(this IEnumerable<TCardGroup> cardList)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return cardList.Sum(static group => group.Size);
    }

    /// <summary>
    /// Gets the deck size of the card list; the summation of all .Size in the card list.
    /// </summary>
    /// <typeparam name="TCardGroup">The CardGroup type.</typeparam>
    /// <typeparam name="TCardGroupName">The type of the name used in the CardGroup.</typeparam>
    /// <param name="cardList">The card list.</param>
    /// <returns>The deck size.</returns>
    [Pure]
    public static int GetNumberOfCards<TCardGroup, TCardGroupName>(this ICardGroupCollection<TCardGroup, TCardGroupName> cardList)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        IEnumerable<TCardGroup> enumerable = cardList;
        return enumerable.GetNumberOfCards<TCardGroup, TCardGroupName>();
    }

    [Pure]
    public static CardList<TCardGroup, TCardGroupName> Cast<TCardGroupOld, TCardGroup, TCardGroupName>(this CardList<TCardGroupOld, TCardGroupName> cardList, Func<TCardGroupOld, TCardGroup> converter)
        where TCardGroupOld : ICardGroup<TCardGroupName>
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var convertedSet = new HashSet<TCardGroup>(cardList.Select(converter));
        return Create<TCardGroup, TCardGroupName>(convertedSet);
    }

    [Pure]
    public static CardList<TCardGroup, TCardGroupName> Fill<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cardList, int targetSize, Func<int, TCardGroup> miscCardGroupFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardSize = cardList.Sum(static group => group.Size);
        Guard.IsLessThanOrEqualTo(cardSize, targetSize);
        var miscSize = targetSize - cardSize;
        return cardList.Change(miscCardGroupFactory(miscSize));
    }
}

public class CardList<TCardGroup, TCardGroupName> : ICardGroupCollection<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private sealed class CardComparer : IEqualityComparer<ICardGroup<TCardGroupName>>
    {
        public static IEqualityComparer<ICardGroup<TCardGroupName>> Instance { get; } = new CardComparer();

        bool IEqualityComparer<ICardGroup<TCardGroupName>>.Equals(ICardGroup<TCardGroupName>? x, ICardGroup<TCardGroupName>? y)
        {
            if (x is null)
            {
                return false;
            }

            if (y is null)
            {
                return false;
            }

            return
                x.Name.Equals(y.Name) &&
                x.Size == y.Size &&
                x.Minimum == y.Minimum &&
                x.Maximum == y.Maximum;
        }

        int IEqualityComparer<ICardGroup<TCardGroupName>>.GetHashCode(ICardGroup<TCardGroupName> obj)
        {
            return HashCode.Combine(obj.Name, obj.Size, obj.Minimum, obj.Maximum);
        }
    }

    private sealed class Comparer : IEqualityComparer<TCardGroup>
    {
        public static IEqualityComparer<TCardGroup> Instance { get; } = new Comparer();

        public bool Equals(TCardGroup? x, TCardGroup? y)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return NameComparer.Instance.Equals(x.Name, y.Name);
        }

        public int GetHashCode([DisallowNull] TCardGroup obj)
        {
            return NameComparer.Instance.GetHashCode(obj.Name);
        }
    }

    private sealed class NameComparer : IEqualityComparer<TCardGroupName>
    {
        public static IEqualityComparer<TCardGroupName> Instance { get; } = new NameComparer();

        public bool Equals(TCardGroupName? x, TCardGroupName? y)
        {
            if(x is null)
            {
                return y is null;
            }

            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] TCardGroupName obj)
        {
            return obj.GetHashCode();
        }
    }

    private HashSet<TCardGroup> Cards { get; }

    public int Count => Cards.Count;

    public IEnumerable<TCardGroupName> CardNames { get; }

    public CardList(IEnumerable<TCardGroup> cards)
    {
        Cards = new HashSet<TCardGroup>(cards.Where(static group => group.Size > 0), Comparer.Instance);
        CardNames = Cards.Select(static group => group.Name).ToList();
    }

    [Pure]
    public bool SetEquals(IEnumerable<TCardGroup> cards)
    {
        var originalSet = new HashSet<ICardGroup<TCardGroupName>>(Cards.Cast<ICardGroup<TCardGroupName>>(), CardComparer.Instance);
        var comparisonSet = new HashSet<ICardGroup<TCardGroupName>>(cards.Cast<ICardGroup<TCardGroupName>>(), CardComparer.Instance);
        return originalSet.SetEquals(comparisonSet);
    }

    [Pure]
    internal CardList<TCardGroup, TCardGroupName> Change(TCardGroupName name, Func<TCardGroup, TCardGroup> change)
    {
        TCardGroup? oldCard = default;
        foreach(var card in Cards)
        {
            if(card.Name.Equals(name))
            {
                oldCard = card;
                break;
            }
        }

        if(oldCard != null)
        {
            var chg = change(oldCard);
            return Change(chg);
        }

        return this;
    }

    [Pure]
    internal CardList<TCardGroup, TCardGroupName> Change(TCardGroup newValue)
    {
        var cards = new HashSet<TCardGroup>(Cards, Comparer.Instance);

        // items are compared by U, so this is removing any old
        // item that have the same U as newValue.
        cards.Remove(newValue);

        // add the newValue that has the data we want.
        cards.Add(newValue);

        return new CardList<TCardGroup, TCardGroupName>(cards);
    }

    [Pure]
    internal CardList<TCardGroup, TCardGroupName> Remove(TCardGroupName cardName)
    {
        var hasCardName = false;

        foreach(var card in Cards)
        {
            if(NameComparer.Instance.Equals(cardName, card.Name))
            {
                hasCardName = true;
                break;
            }
        }

        if(!hasCardName)
        {
            return this;
        }

        var cards = new HashSet<TCardGroup>(Cards, Comparer.Instance);
        cards.RemoveWhere(group => NameComparer.Instance.Equals(group.Name, cardName));
        return new CardList<TCardGroup, TCardGroupName>(cards);
    }

    public IEnumerator<TCardGroup> GetEnumerator()
    {
        IEnumerable<TCardGroup> enumerable = Cards;
        return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerable = Cards;
        return enumerable.GetEnumerator();
    }
}
