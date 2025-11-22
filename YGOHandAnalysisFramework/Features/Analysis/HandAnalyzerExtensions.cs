using CommunityToolkit.Diagnostics;
using System.Diagnostics.Contracts;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Features.Analysis;

public static class HandAnalyzerExtensions
{
    extension<TCardGroup, TCardGroupName>(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
        where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        /// <summary>
        /// Produces a new version of <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> but with <paramref name="hand"/> removed.
        /// </summary>
        /// <remarks>
        /// The hand size of the returned <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> is the hand size of <paramref name="handAnalyzer"/>.
        /// There is another version of <c>Excavate</c> where a different hand size can be specified.
        /// </remarks>
        /// <param name="handAnalyzer">The original hand analyzer.</param>
        /// <param name="hand">The cards in <paramref name="hand"/> will not be present in the returned <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c>, but the rest of the cards from <paramref name="handAnalyzer"/> will be.</param>
        /// <returns>A new <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> based on <paramref name="handAnalyzer"/> but without the cards in <paramref name="hand"/>.</returns>
        [Pure]
        public HandAnalyzer<TCardGroup, TCardGroupName> Excavate(HandCombination<TCardGroupName> hand)
        {
            var cardList = CardList
                .Create(handAnalyzer)
                .RemoveHand(hand);
            var handSize = Math.Min(cardList.GetNumberOfCards(), handAnalyzer.HandSize);

            Guard.IsGreaterThan(handSize, 0);

            return cardList
                .CreateHandAnalyzerBuildArgs(handAnalyzer.AnalyzerName, handSize)
                .CreateHandAnalyzer();
        }

        /// <summary>
        /// Produces a new version of <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> but with <paramref name="hand"/> removed and a hand size of <paramref name="handSize"/>.
        /// </summary>
        /// <param name="handAnalyzer">The original hand analyzer.</param>
        /// <param name="hand">The cards in <paramref name="hand"/> will not be present in the returned <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c>, but the rest of the cards from <paramref name="handAnalyzer"/> will be.</param>
        /// <param name="handSize">The hand size of the returned <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c>.</param>
        /// <returns>A new <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> based on <paramref name="handAnalyzer"/> but without the cards in <paramref name="hand"/>. The hand size is <paramref name="handSize"/>.</returns>
        [Pure]
        public HandAnalyzer<TCardGroup, TCardGroupName> Excavate(HandCombination<TCardGroupName> hand, int handSize)
        {
            var cardList = CardList
                .Create(handAnalyzer)
                .RemoveHand(hand);
            var actualHandSize = Math.Min(cardList.GetNumberOfCards(), handSize);

            Guard.IsGreaterThan(actualHandSize, 0);

            return cardList
                .CreateHandAnalyzerBuildArgs(handAnalyzer.AnalyzerName, actualHandSize)
                .CreateHandAnalyzer();
        }
    }

    extension<TCardGroup, TCardGroupName>(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {

        public CardList<CardGroup<TCardGroupName>, TCardGroupName> ConvertToCardGroup()
        {
            return CardList
                .Create(handAnalyzer)
                .Cast(CardGroup.CreateFrom<TCardGroup, TCardGroupName>);
        }

        /// <summary>
        /// Calculates the probability of drawing <paramref name="hand"/> and <paramref name="probabilityOfOtherEvent"/>, i.e. P(<paramref name="hand"/> and <paramref name="probabilityOfOtherEvent"/>).
        /// </summary>
        /// <param name="hand">The hand whose probability is being calculated.</param>
        /// <param name="probabilityOfOtherEvent">The probability of another event happening.</param>
        /// <returns>The probability of drawing <paramref name="hand"/> and <paramref name="probabilityOfOtherEvent"/>.</returns>
        [Pure]
        public double CalculateProbability(HandCombination<TCardGroupName> hand, double probabilityOfOtherEvent)
        {
            if (probabilityOfOtherEvent > 0)
            {
                return handAnalyzer.CalculateProbability(hand) * probabilityOfOtherEvent;
            }

            return 0.0;
        }

        /// <summary>
        /// <para>Calculates the probability of drawing all hands that match <paramref name="filter"/>.</para>
        /// <para>You can also access <paramref name="args"/> in <paramref name="filter"/>.</para>
        /// </summary>
        /// <typeparam name="TArgs">The type of the data being passed to <paramref name="filter"/>.</typeparam>
        /// <param name="args">The data being passed to <paramref name="filter"/>.</param>
        /// <param name="filter">The filter, which determines which hands to include and not.</param>
        /// <returns>The probability of drawing all hands that match <paramref name="filter"/>.</returns>
        [Pure]
        public double CalculateProbability<TArgs>(TArgs args, Func<HandCombination<TCardGroupName>, TArgs, bool> filter)
        {
            var prob = 0.0;

            foreach(var hand in handAnalyzer.Combinations)
            {
                if(filter(hand, args))
                {
                    prob += handAnalyzer.CalculateProbability(hand);
                }
            }

            return prob;
        }

        /// <summary>
        /// <para>Calculates the probability of drawing all hands that match <paramref name="filter"/>.</para>
        /// <para>You can also access <paramref name="handAnalyzer"/> in <paramref name="filter"/>.</para>
        /// </summary>
        /// <param name="filter">The filter, which determines which hands to include and not.</param>
        /// <returns>The probability of drawing all hands that match <paramref name="filter"/>.</returns>
        [Pure]
        public double CalculateProbability(Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, bool> filter)
        {
            var prob = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                if (filter(handAnalyzer, hand))
                {
                    prob += handAnalyzer.CalculateProbability(hand);
                }
            }

            return prob;
        }

        /// <summary>
        /// <para>Calculates the probability of drawing all hands that match <paramref name="filter"/>.</para>
        /// <para>You can also access <paramref name="handAnalyzer"/> and <paramref name="args"/> in <paramref name="filter"/>.</para>
        /// </summary>
        /// <typeparam name="TArgs">The type of the data being passed to <paramref name="filter"/>.</typeparam>
        /// <param name="args">The data being passed to <paramref name="filter"/>.</param>
        /// <param name="filter">The filter, which determines which hands to include and not.</param>
        /// <returns>The probability of drawing all hands that match <paramref name="filter"/>.</returns>
        [Pure]
        public double CalculateProbability<TArgs>(TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TArgs, bool> filter)
        {
            var prob = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                if (filter(handAnalyzer, hand, args))
                {
                    prob += handAnalyzer.CalculateProbability(hand);
                }
            }

            return prob;
        }

        /// <summary>
        /// Calculates the total probability of all hand combinations that satisfy the specified filter.
        /// </summary>
        /// <param name="filter">An object that defines the criteria used to select hand combinations for probability calculation. Cannot be
        /// null.</param>
        /// <returns>The sum of probabilities for all hand combinations that match the filter criteria. Returns 0.0 if no
        /// combinations match.</returns>
        [Pure]
        public double CalculateProbability(IFilter<HandCombination<TCardGroupName>> filter)
        {
            var prob = 0.0;

            foreach (var hand in filter.GetResults(handAnalyzer.Combinations))
            {
                prob += handAnalyzer.CalculateProbability(hand);
            }

            return prob;
        }

        /// <summary>
        /// Calculates the expected value over all possible hand combinations using the specified value selector
        /// function.
        /// </summary>
        /// <remarks>The expected value is calculated by multiplying the probability of each hand
        /// combination by the value returned from the selector, and summing these products for all combinations where
        /// the value is greater than zero. This method does not consider combinations with non-positive
        /// values.</remarks>
        /// <param name="valueSelector">A function that assigns a numeric value to each hand combination. Only combinations with a positive value
        /// are included in the calculation.</param>
        /// <returns>The expected value computed as the sum of the probability-weighted values of all hand combinations for which
        /// the selector returns a positive value.</returns>
        [Pure]
        public double CalculateExpectedValue(Func<HandCombination<TCardGroupName>, double> valueSelector)
        {
            var expectedValue = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                var value = valueSelector(hand);
                if (value > 0)
                {
                    expectedValue += handAnalyzer.CalculateProbability(hand) * value;
                }
            }

            return expectedValue;
        }

        /// <summary>
        /// Calculates the expected value over all possible hand combinations using a specified value selector function.
        /// </summary>
        /// <remarks>Only hand combinations for which the value selector returns a value greater than 0
        /// are included in the expected value calculation.</remarks>
        /// <typeparam name="TArgs">The type of the additional arguments passed to the value selector function.</typeparam>
        /// <param name="args">An object containing additional arguments to be supplied to the value selector function for each hand
        /// combination.</param>
        /// <param name="valueSelector">A function that computes a value for a given hand combination and the provided arguments. The function
        /// should return a non-negative value representing the outcome for that combination.</param>
        /// <returns>The sum of the probabilities of each hand combination multiplied by the value returned from the value
        /// selector function. Returns 0.0 if no hand combinations yield a positive value.</returns>
        [Pure]
        public double CalculateExpectedValue<TArgs>(TArgs args, Func<HandCombination<TCardGroupName>, TArgs, double> valueSelector)
        {
            var expectedValue = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                var value = valueSelector(hand, args);
                if (value > 0)
                {
                    expectedValue += handAnalyzer.CalculateProbability(hand) * value;
                }
            }

            return expectedValue;
        }

        /// <summary>
        /// Calculates the expected value of all hand combinations using the specified value selector function.
        /// </summary>
        /// <remarks>Only hand combinations for which the value selector returns a value greater than zero
        /// are included in the calculation. This method is pure and does not modify the state of the analyzer or its
        /// combinations.</remarks>
        /// <param name="valueSelector">A function that computes the value for a given hand combination. The function receives the current hand
        /// analyzer and a hand combination, and returns a double representing the value associated with that
        /// combination.</param>
        /// <returns>The sum of the probabilities of each hand combination multiplied by its corresponding value, as determined
        /// by the value selector function.</returns>
        [Pure]
        public double CalculateExpectedValue(Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, double> valueSelector)
        {
            var expectedValue = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                var value = valueSelector(handAnalyzer, hand);
                if (value > 0)
                {
                    expectedValue += handAnalyzer.CalculateProbability(hand) * value;
                }
            }

            return expectedValue;
        }

        /// <summary>
        /// Calculates the expected value over all hand combinations using a user-provided value selector function.
        /// </summary>
        /// <remarks>Only hand combinations for which the value selector returns a value greater than 0
        /// are included in the expected value calculation.</remarks>
        /// <typeparam name="TArgs">The type of the additional arguments passed to the value selector function.</typeparam>
        /// <param name="args">An object containing additional arguments to be supplied to the value selector function for each hand
        /// combination.</param>
        /// <param name="valueSelector">A function that computes the value for a given hand combination. The function receives the hand analyzer,
        /// the current hand combination, and the additional arguments, and returns a double representing the value for
        /// that combination.</param>
        /// <returns>The sum of the probabilities of each hand combination multiplied by the value returned from the value
        /// selector function. Returns 0.0 if no hand combinations yield a positive value.</returns>
        [Pure]
        public double CalculateExpectedValue<TArgs>(TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TArgs, double> valueSelector)
        {
            var expectedValue = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                var value = valueSelector(handAnalyzer, hand, args);
                if (value > 0)
                {
                    expectedValue += handAnalyzer.CalculateProbability(hand) * value;
                }
            }

            return expectedValue;
        }

        /// <summary>
        /// Calculates the expected value of all hand combinations that satisfy a specified filter, using a provided
        /// value selector function.
        /// </summary>
        /// <remarks>This method computes a probability-weighted average of the values returned by
        /// <paramref name="valueSelector"/> for all hand combinations that satisfy <paramref name="filter"/>. If no
        /// combinations match the filter, the method returns 0.0.</remarks>
        /// <param name="filter">A predicate that determines whether a given hand combination should be included in the calculation. Only
        /// combinations for which this function returns <see langword="true"/> are considered.</param>
        /// <param name="valueSelector">A function that selects the numeric value to use for each hand combination included in the calculation.</param>
        /// <returns>The expected value of the filtered hand combinations, or 0.0 if no combinations match the filter.</returns>
        [Pure]
        public double CalculateExpectedValueOfSubset(Func<HandCombination<TCardGroupName>, bool> filter, Func<HandCombination<TCardGroupName>, double> valueSelector)
        {
            var ev = 0.0;
            var totalProb = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                if (!filter(hand))
                {
                    continue;
                }

                var value = valueSelector(hand);
                var prob = handAnalyzer.CalculateProbability(hand);
                totalProb += prob;
                ev += prob * value;
            }

            if(totalProb == 0.0)
            {
                return 0.0;
            }

            return ev / totalProb;
        }

        /// <summary>
        /// Calculates the expected value of all hand combinations that satisfy a specified filter, using a provided
        /// value selector function.
        /// </summary>
        /// <remarks>This method evaluates each hand combination using the specified filter and value
        /// selector, weighting each value by the probability of the hand. The expected value is normalized by the total
        /// probability of the filtered subset. If no hands match the filter, the method returns 0.0.</remarks>
        /// <typeparam name="TArgs">The type of the argument object passed to the filter and value selector functions.</typeparam>
        /// <param name="args">An argument object that provides contextual information to the filter and value selector functions.</param>
        /// <param name="filter">A function that determines whether a given hand combination should be included in the calculation. The
        /// function receives a hand combination and the argument object, and returns <see langword="true"/> to include
        /// the hand; otherwise, <see langword="false"/>.</param>
        /// <param name="valueSelector">A function that computes the value associated with a given hand combination. The function receives a hand
        /// combination and the argument object, and returns a <see cref="double"/> representing the value for that
        /// hand.</param>
        /// <returns>The expected value, as a <see cref="double"/>, of all hand combinations that satisfy the filter. Returns 0.0
        /// if no combinations match the filter.</returns>
        [Pure]
        public double CalculateExpectedValueOfSubset<TArgs>(TArgs args, Func<HandCombination<TCardGroupName>, TArgs, bool> filter, Func<HandCombination<TCardGroupName>, TArgs, double> valueSelector)
        {
            var ev = 0.0;
            var totalProb = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                if (!filter(hand, args))
                {
                    continue;
                }

                var value = valueSelector(hand, args);
                var prob = handAnalyzer.CalculateProbability(hand);
                totalProb += prob;
                ev += prob * value;
            }

            if (totalProb == 0.0)
            {
                return 0.0;
            }

            return ev / totalProb;
        }

        /// <summary>
        /// Calculates the expected value of all hand combinations that satisfy a specified filter, using a provided
        /// value selector function.
        /// </summary>
        /// <remarks>The expected value is calculated as the probability-weighted average of the values
        /// returned by <paramref name="valueSelector"/> for all combinations accepted by <paramref name="filter"/>. If
        /// no combinations match the filter, the method returns 0.0.</remarks>
        /// <param name="filter">A function that determines whether a given hand combination should be included in the calculation. The
        /// function receives the hand analyzer and a hand combination, and returns <see langword="true"/> to include
        /// the combination; otherwise, <see langword="false"/>.</param>
        /// <param name="valueSelector">A function that computes the value associated with each hand combination. The function receives the hand
        /// analyzer and a hand combination, and returns a <see cref="double"/> representing the value for that
        /// combination.</param>
        /// <returns>The expected value, as a <see cref="double"/>, of all hand combinations that pass the filter. Returns 0.0 if
        /// no combinations satisfy the filter.</returns>
        [Pure]
        public double CalculateExpectedValueOfSubset(Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, bool> filter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, double> valueSelector)
        {
            var ev = 0.0;
            var totalProb = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                if (!filter(handAnalyzer, hand))
                {
                    continue;
                }

                var value = valueSelector(handAnalyzer, hand);
                var prob = handAnalyzer.CalculateProbability(hand);
                totalProb += prob;
                ev += prob * value;
            }

            if (totalProb == 0.0)
            {
                return 0.0;
            }

            return ev / totalProb;
        }

        /// <summary>
        /// Calculates the expected value over a subset of hand combinations that satisfy a specified filter condition.
        /// </summary>
        /// <remarks>This method iterates over all possible hand combinations, applying the filter and
        /// value selector to each. The expected value is calculated only from the subset of hands for which the filter
        /// returns <see langword="true"/>. If no hands match the filter, the method returns 0.0.</remarks>
        /// <typeparam name="TArgs">The type of the argument object passed to the filter and value selector functions.</typeparam>
        /// <param name="args">An argument object that provides contextual information to the filter and value selector functions.</param>
        /// <param name="filter">A function that determines whether a given hand combination should be included in the subset. The function
        /// receives the hand analyzer, the hand combination, and the argument object, and returns <see
        /// langword="true"/> to include the hand; otherwise, <see langword="false"/>.</param>
        /// <param name="valueSelector">A function that computes the value associated with a given hand combination. The function receives the hand
        /// analyzer, the hand combination, and the argument object, and returns a <see cref="double"/> representing the
        /// value for that hand.</param>
        /// <returns>The expected value, as a <see cref="double"/>, computed as the probability-weighted average of the values
        /// for all hand combinations that satisfy the filter. Returns 0.0 if no combinations match the filter.</returns>
        [Pure]
        public double CalculateExpectedValueOfSubset<TArgs>(TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TArgs, bool> filter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TArgs, double> valueSelector)
        {
            var ev = 0.0;
            var totalProb = 0.0;

            foreach (var hand in handAnalyzer.Combinations)
            {
                if (!filter(handAnalyzer, hand, args))
                {
                    continue;
                }

                var value = valueSelector(handAnalyzer, hand, args);
                var prob = handAnalyzer.CalculateProbability(hand);
                totalProb += prob;
                ev += prob * value;
            }

            if (totalProb == 0.0)
            {
                return 0.0;
            }

            return ev / totalProb;
        }

        [Pure]
        public double Aggregate<TAggregate>(Func<HandCombination<TCardGroupName>, TAggregate> aggregator, Func<IReadOnlyDictionary<HandCombination<TCardGroupName>, TAggregate>, double> calculator)
        {
            var aggregateValues = new Dictionary<HandCombination<TCardGroupName>, TAggregate>(handAnalyzer.Combinations.Count);

            foreach (var hand in handAnalyzer.Combinations)
            {
                aggregateValues[hand] = aggregator(hand);
            }

            return calculator(aggregateValues);
        }

        [Pure]
        public double Aggregate<TAggregate>(Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAggregate> aggregator, Func<IReadOnlyDictionary<HandCombination<TCardGroupName>, TAggregate>, double> calculator)
        {
            var aggregateValues = new Dictionary<HandCombination<TCardGroupName>, TAggregate>(handAnalyzer.Combinations.Count);

            foreach (var hand in handAnalyzer.Combinations)
            {
                aggregateValues[hand] = aggregator(hand, handAnalyzer);
            }

            return calculator(aggregateValues);
        }
    }
}
