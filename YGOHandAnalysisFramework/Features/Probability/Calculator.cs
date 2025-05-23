﻿using CommunityToolkit.Diagnostics;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Features.Probability;

internal static class Calculator
{
    internal static double CalculateProbability<TCardGroup, TCardGroupName>(IEnumerable<TCardGroup> cardGroups, IEnumerable<HandCombination<TCardGroupName>> handCombinations, int deckSize, int handSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return CalculateProbability(cardGroups, new HashSet<HandCombination<TCardGroupName>>(handCombinations), deckSize, handSize);
    }

    internal static double CalculateProbability<TCardGroup, TCardGroupName>(IEnumerable<TCardGroup> cardGroups, IReadOnlyCollection<HandCombination<TCardGroupName>> handCombinations, int deckSize, int handSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardGroupByName = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, cardGroups);
        var totalProb = 0.0;

        foreach (var handPermutation in handCombinations)
        {
            var currentCardGroup = new List<CardGroup<TCardGroupName>>(handPermutation.CardNames.Count);

            foreach (var cardGroup in handPermutation.CardNames)
            {
                if (!cardGroupByName.TryGetValue(cardGroup.HandName, out var originalCardGroup))
                {
                    throw new Exception();
                }

                currentCardGroup.Add(new CardGroup<TCardGroupName>
                {
                    Name = originalCardGroup.Name,
                    Size = originalCardGroup.Size,
                    Minimum = cardGroup.MinimumSize,
                    Maximum = cardGroup.MaximumSize
                });
            }

            var prob = Calculate<CardGroup<TCardGroupName>, TCardGroupName>(currentCardGroup, deckSize, handSize);
            totalProb += prob;
        }

        return totalProb;
    }

    internal static double CalculateProbability<TCardGroup, TCardGroupName>(IEnumerable<TCardGroup> cardGroups, HandCombination<TCardGroupName> handPermutation, int deckSize, int handSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardGroupByName = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, cardGroups);
        var currentCardGroup = new List<CardGroup<TCardGroupName>>(handPermutation.CardNames.Count);

        foreach (var cardGroup in handPermutation.CardNames)
        {
            if (!cardGroupByName.TryGetValue(cardGroup.HandName, out var originalCardGroup))
            {
                throw new Exception();
            }

            currentCardGroup.Add(new CardGroup<TCardGroupName>
            {
                Name = cardGroup.HandName,
                Size = originalCardGroup.Size,
                Minimum = cardGroup.MinimumSize,
                Maximum = cardGroup.MaximumSize,
            });
        }

        return Calculate<CardGroup<TCardGroupName>, TCardGroupName>(currentCardGroup, deckSize, handSize);
    }

    private static double Calculate<TCardGroup, TCardGroupName>(List<TCardGroup> cardGroups, int deckSize, int handSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        Validation<TCardGroup, TCardGroupName>(cardGroups, deckSize, handSize);

        var stack = new Stack<TCardGroup>(cardGroups);

        var top = CalculateProbabilityOfHand<TCardGroup, TCardGroupName>(handSize, stack);
        var bottom = new CardChance()
        {
            N = deckSize,
            K = handSize
        }.Calculate();
        return top / bottom;
    }

    private static void Validation<TCardGroup, TCardGroupName>(List<TCardGroup> cardGroups, int deckSize, int handSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        {
            var groupSize = cardGroups.Sum(static group => group.Size);

            if (deckSize < groupSize)
            {
                throw new Exception($"Card group aggregate size ({groupSize}) cannot be greater than deck size ({deckSize}).");
            }

            if (deckSize < handSize)
            {
                throw new Exception($"Hand size ({handSize}) cannot be greater than deck size ({deckSize}).");
            }
        }

        {
            foreach (var group in cardGroups)
            {
                if (group.Minimum < 0)
                {
                    throw new Exception($"Minimum ({group.Minimum}) in {group.Name} must be greater than 0.");
                }

                if (group.Maximum > group.Size)
                {
                    throw new Exception($"Maximum ({group.Maximum}) in {group.Name} cannot be greater than size ({group.Size}).");
                }
            }
        }
    }

    private static double CalculateProbabilityOfHand<TCardGroup, TCardGroupName>(int maxHandSize, Stack<TCardGroup> cardGroups)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var hand = new Stack<CardChance>();
        return Impl(hand, 0, maxHandSize, cardGroups);
        
        static double Impl(Stack<CardChance> hand, int currentHandSize, int maxHandSize, Stack<TCardGroup> cardGroups)
        {
            if (cardGroups.Count == 0 || currentHandSize >= maxHandSize)
            {
                if (currentHandSize == maxHandSize)
                {
                    foreach (var group in cardGroups)
                    {
                        if (group.Minimum != 0)
                        {
                            return 0;
                        }
                    }
                }
                else if (currentHandSize > maxHandSize)
                {
                    return 0;
                }

                var chance = 1.0;
                foreach (var group in hand)
                {
                    chance *= group.Calculate();
                }

                return chance;
            }

            {
                var group = cardGroups.Pop();
                var probs = 0.0;

                for (int i = group.Minimum, length = group.Maximum; i <= length; i++)
                {
                    hand.Push(new CardChance()
                    {
                        N = group.Size,
                        K = i
                    });

                    probs += Impl(hand, currentHandSize + i, maxHandSize, cardGroups);

                    hand.Pop();
                }

                cardGroups.Push(group);

                return probs;
            }
        }
    }

    private sealed class CardChance
    {
        public readonly static double[] FactorialCache;

        static CardChance()
        {
            double factorial = 1.0;
            FactorialCache = new double[171];

            for (var i = 0; i < FactorialCache.Length; i++)
            {
                FactorialCache[i] = factorial;
                factorial *= i + 1.0;
            }
        }

        public int N { get; init; }
        public int K { get; init; }

        public double Calculate()
        {
            var top = Factorial(N);
            var bottom = (Factorial(K) * Factorial(N - K));
            return top / bottom;
        }

        private static double Factorial(int number)
        {
            Guard.HasSizeGreaterThan(FactorialCache, number);
            return FactorialCache[number];
        }
    }
}