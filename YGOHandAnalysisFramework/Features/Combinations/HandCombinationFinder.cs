using System.Collections.Immutable;
using YGOHandAnalysisFramework.Data;

namespace YGOHandAnalysisFramework.Features.Combinations;

internal static class HandCombinationFinder
{
    private sealed class HandStackWithSizeCounter<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        private int Size;
        private readonly Stack<HandElement<TCardGroupName>> Storage = new(128);

        public HandElement<TCardGroupName>[] GetHandPermutations()
        {
            return [.. Storage];
        }

        public int GetHandSize()
        {
            return Size;
        }

        public HandElement<TCardGroupName> Pop()
        {
            var pop = Storage.Pop();
            Size -= pop.MinimumSize;
            return pop;
        }

        public void Push(HandElement<TCardGroupName> handPermutation)
        {
            Size += handPermutation.MinimumSize;
            Storage.Push(handPermutation);
        }
    }

    internal static IImmutableSet<HandCombination<TCardGroupName>> GetCombinations<TCardGroup, TCardGroupName>(int startingHandSize, IEnumerable<TCardGroup> cardGroups)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var hand = cardGroups
                .Select(static group => new HandElement<TCardGroupName>
                {
                    HandName = group.Name,
                    MinimumSize = group.Minimum,
                    MaximumSize = group.Maximum,
                })
                .ToHashSet();

        return GetCombinations(startingHandSize, hand);
    }

    internal static IImmutableSet<HandCombination<TCardGroupName>> GetCombinations<TCardGroupName>(int startingHandSize, IEnumerable<HandElement<TCardGroupName>> cardGroups)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return GetCombinations(startingHandSize, cardGroups.ToHashSet());
    }

    private static ImmutableHashSet<HandCombination<TCardGroupName>> GetCombinations<TCardGroupName>(int startingHandSize, HashSet<HandElement<TCardGroupName>> cardGroups)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        static void Recursive(int startingHandSize, HandStackWithSizeCounter<TCardGroupName> hand, List<HandElement<TCardGroupName>[]> storage, Stack<HandElement<TCardGroupName>> start)
        {
            if (start.Count == 0)
            {
                if (hand.GetHandSize() != startingHandSize)
                {
                    return;
                }

                storage.Add(hand.GetHandPermutations());
                return;
            }

            var group = start.Pop();
            var handName = group.HandName;
            var maxSize = group.MaximumSize;

            for (int i = group.MinimumSize, length = group.MaximumSize; i <= length; i++)
            {
                hand.Push(new HandElement<TCardGroupName>()
                {
                    HandName = handName,
                    MinimumSize = i,
                    MaximumSize = maxSize,
                });

                Recursive(startingHandSize, hand, storage, start);

                hand.Pop();
            }

            start.Push(group);
        }

        var permutations = new List<HandElement<TCardGroupName>[]>(32768);
        var stack = new HandStackWithSizeCounter<TCardGroupName>();
        var start = new Stack<HandElement<TCardGroupName>>(cardGroups);
        Recursive(startingHandSize, stack, permutations, start);

        var emptyHand = new HashSet<HandElement<TCardGroupName>>(cardGroups.Count);

        foreach(var card in cardGroups)
            {
            emptyHand.Add(new HandElement<TCardGroupName>()
            {
                HandName = card.HandName,
                MinimumSize = 0,
                MaximumSize = card.MaximumSize,
            });
        }

        var completeSet = new List<HandCombination<TCardGroupName>>(permutations.Count);

        foreach (var handPermutation in permutations)
        {
            // We fill the hand with empties, because to compare two hands, they must be hashed with the same set of data but differ in their card quantity distribution.
            // e.g, hand 1 = [a:1, b:0, c:2, d:0, ...]
            // v.s. hand 2 = [a:0, b:1, c:2, d:1, ...]
            // sorting is handled by HandCombination<TCardGroupName>
            var handWithEmpties = new HashSet<HandElement<TCardGroupName>>(handPermutation, HandCombinationNameComparer<TCardGroupName>.Default);
            
            foreach(var empty in emptyHand)
            {
                handWithEmpties.Add(empty);
            }

            completeSet.Add(new HandCombination<TCardGroupName>(handWithEmpties));
        }

        return [.. completeSet];
    }
}
