using YGOHandAnalysisFramework.Data.Operations;
using System.Collections.Immutable;

namespace YGOHandAnalysisFramework.Features.Combinations;

public readonly struct HandCombination<TCardGroupName> : IEquatable<HandCombination<TCardGroupName>>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    internal readonly ReadOnlyMemory<byte> Hand { get; }
    internal readonly IReadOnlyList<TCardGroupName> CardNames { get; }

    public HandCombination(ReadOnlyMemory<byte> hand, IReadOnlyList<TCardGroupName> cards)
    {
        Hand = hand;
        CardNames = cards;
    }

    public readonly IEnumerable<TCardGroupName> GetCardsInHand()
    {
        foreach(var (amount, card) in this)
        {
            if(amount > 0)
            {
                yield return card;
            }
        }
    }

    public readonly bool Equals(HandCombination<TCardGroupName> other)
    {
        return Hand.Equals(other.Hand) && CardNames == other.CardNames;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is HandCombination<TCardGroupName> other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Hand, CardNames);
    }

    public static bool operator ==(HandCombination<TCardGroupName> left, HandCombination<TCardGroupName> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HandCombination<TCardGroupName> left, HandCombination<TCardGroupName> right)
    {
        return !(left == right);
    }

    public readonly Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public struct Enumerator
    {
        private int _index;
        private readonly ReadOnlyMemory<byte> _hand;
        private readonly IReadOnlyList<TCardGroupName> _deck;

        public readonly (int Amount, TCardGroupName CardName) Current => (_hand.Span[_index], _deck[_index]);

        public Enumerator(HandCombination<TCardGroupName> handCombination)
        {
            _hand = handCombination.Hand;
            _deck = handCombination.CardNames;
            _index = -1;
        }

        public bool MoveNext()
        {
            _index++;
            return _index < _hand.Length;
        }
    }
}
