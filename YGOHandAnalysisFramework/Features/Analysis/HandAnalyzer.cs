using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Probability;
using YGOHandAnalysisFramework.Features.Caching;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using System.Diagnostics.Contracts;

namespace YGOHandAnalysisFramework.Features.Analysis;

public static class HandAnalyzer
{
    [Pure]
    public static HandAnalyzer<TCardGroup, TCardGroupName> CreateHandAnalyzer<TCardGroup, TCardGroupName>(this HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArguments)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzer<TCardGroup, TCardGroupName>(buildArguments);
    }

    [Pure]
    public static HandAnalyzer<TCardGroup, TCardGroupName> CreateHandAnalyzer<TCardGroup, TCardGroupName>(this HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArguments, HandAnalyzerLoader<TCardGroup, TCardGroupName> loader)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if (loader.TryLoadHandAnalyzer(buildArguments).GetResult(out var loadedAnalyzer, out _))
        {
            return loadedAnalyzer;
        }

        var handAnalyzer = CreateHandAnalyzer(buildArguments);
        loader.CreateCache(handAnalyzer);

        return handAnalyzer;
    }

    [Pure]
    public static IReadOnlyDictionary<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>> CreateInParallel<TCardGroup, TCardGroupName>(IEnumerable<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>> buildArguments, HandAnalyzerLoader<TCardGroup, TCardGroupName> loader)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var hardLoads = new List<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>>();
        var analyzers = new Dictionary<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>>();

        foreach (var args in buildArguments)
        {
            if (!loader.TryLoadHandAnalyzer(args).GetResult(out var loadedAnalyzer, out _))
            {
                hardLoads.Add(args);
                continue;
            }

            analyzers[args] = loadedAnalyzer;
        }

        var hardLoadedAnalyzers = CreateInParallel(hardLoads);

        foreach (var (args, analyzer) in hardLoadedAnalyzers)
        {
            analyzers[args] = analyzer;
            loader.CreateCache(analyzer);
        }

        return analyzers;
    }

    [Pure]
    public static IReadOnlyDictionary<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>> CreateInParallel<TCardGroup, TCardGroupName>(IEnumerable<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>> buildArguments)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var analyzers = new ConcurrentBag<(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>)>();

        Parallel.ForEach(buildArguments, buildArgument =>
        {
            var analyzer = new HandAnalyzer<TCardGroup, TCardGroupName>(buildArgument);
            analyzers.Add((buildArgument, analyzer));
        });

        var analyzerByBuildArgs = new Dictionary<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>>();

        foreach (var (buildArgs, analyzer) in analyzers)
        {
            analyzerByBuildArgs[buildArgs] = analyzer;
        }

        return analyzerByBuildArgs;
    }

    /// <summary>
    /// Produces a new version of <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> but with <paramref name="hand"/> removed.
    /// </summary>
    /// <remarks>
    /// The hand size of the returned <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> is the hand size of <paramref name="originalAnalyzer"/>.
    /// There is another version of <c>Excavate</c> where a different hand size can be specified.
    /// </remarks>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="originalAnalyzer">The original hand analyzer.</param>
    /// <param name="hand">The cards in <paramref name="hand"/> will not be present in the returned <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c>, but the rest of the cards from <paramref name="originalAnalyzer"/> will be.</param>
    /// <returns>A new <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> based on <paramref name="originalAnalyzer"/> but without the cards in <paramref name="hand"/>.</returns>
    [Pure]
    public static HandAnalyzer<TCardGroup, TCardGroupName> Excavate<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> originalAnalyzer, HandCombination<TCardGroupName> hand)
        where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardList = CardList
            .Create(originalAnalyzer)
            .RemoveHand(hand);
        var handSize = Math.Min(cardList.GetNumberOfCards(), originalAnalyzer.HandSize);

        Guard.IsGreaterThan(handSize, 0);

        return cardList
            .CreateHandAnalyzerBuildArgs(originalAnalyzer.AnalyzerName, handSize)
            .CreateHandAnalyzer();
    }

    /// <summary>
    /// Produces a new version of <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> but with <paramref name="hand"/> removed and a hand size of <paramref name="handSize"/>.
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="originalAnalyzer">The original hand analyzer.</param>
    /// <param name="hand">The cards in <paramref name="hand"/> will not be present in the returned <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c>, but the rest of the cards from <paramref name="originalAnalyzer"/> will be.</param>
    /// <param name="handSize">The hand size of the returned <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c>.</param>
    /// <returns>A new <c>HandAnalyzer&lt;TCardGroup, TCardGroupName&gt;</c> based on <paramref name="originalAnalyzer"/> but without the cards in <paramref name="hand"/>. The hand size is <paramref name="handSize"/>.</returns>
    [Pure]
    public static HandAnalyzer<TCardGroup, TCardGroupName> Excavate<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> originalAnalyzer, HandCombination<TCardGroupName> hand, int handSize)
        where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardList = CardList
            .Create(originalAnalyzer)
            .RemoveHand(hand);
        var actualHandSize = Math.Min(cardList.GetNumberOfCards(), handSize);

        Guard.IsGreaterThan(actualHandSize, 0);

        return cardList
            .CreateHandAnalyzerBuildArgs(originalAnalyzer.AnalyzerName, actualHandSize)
            .CreateHandAnalyzer();
    }

    /// <summary>
    /// Calculates the probability of drawing all possible hands.
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <returns>The probability of drawing all possible hands.</returns>
    [Pure]
    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, handAnalyzer.Combinations, handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    /// <summary>
    /// Calculates the probability of drawing <paramref name="hand"/>.
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="hand">The hand whose probability is being calculated.</param>
    /// <returns>The probability of drawing <paramref name="hand"/>.</returns>
    [Pure]
    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, HandCombination<TCardGroupName> hand)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, hand, handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    /// <summary>
    /// Calculates the probability of drawing <paramref name="hand"/> and <paramref name="probabilityOfOtherEvent"/>, i.e. P(<paramref name="hand"/> and <paramref name="probabilityOfOtherEvent"/>).
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="hand">The hand whose probability is being calculated.</param>
    /// <param name="probabilityOfOtherEvent">The probability of another event happening.</param>
    /// <returns>The probability of drawing <paramref name="hand"/> and <paramref name="probabilityOfOtherEvent"/>.</returns>
    [Pure]
    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, HandCombination<TCardGroupName> hand, double probabilityOfOtherEvent)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if(probabilityOfOtherEvent > 0)
        {
            return handAnalyzer.CalculateProbability(hand) * probabilityOfOtherEvent;
        }

        return 0.0;
    }

    /// <summary>
    /// Calculates the probability of drawing all hands that match <paramref name="filter"/>.
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="filter">The filter, which determines which hands to include and not.</param>
    /// <returns>The probability of drawing all hands that match <paramref name="filter"/>.</returns>
    [Pure]
    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandCombination<TCardGroupName>, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, handAnalyzer.Combinations.Where(filter), handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    /// <summary>
    /// <para>Calculates the probability of drawing all hands that match <paramref name="filter"/>.</para>
    /// <para>You can also access <paramref name="args"/> in <paramref name="filter"/>.</para>
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <typeparam name="TArgs">The type of the data being passed to <paramref name="filter"/>.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="args">The data being passed to <paramref name="filter"/>.</param>
    /// <param name="filter">The filter, which determines which hands to include and not.</param>
    /// <returns>The probability of drawing all hands that match <paramref name="filter"/>.</returns>
    [Pure]
    public static double CalculateProbability<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandCombination<TCardGroupName>, TArgs, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, GetCombinations(), handAnalyzer.DeckSize, handAnalyzer.HandSize);

        IEnumerable<HandCombination<TCardGroupName>> GetCombinations()
        {
            foreach (var hand in handAnalyzer.Combinations)
            {
                if (filter(hand, args))
                {
                    yield return hand;
                }
            }
        }
    }

    /// <summary>
    /// <para>Calculates the probability of drawing all hands that match <paramref name="filter"/>.</para>
    /// <para>You can also access <paramref name="handAnalyzer"/> in <paramref name="filter"/>.</para>
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="filter">The filter, which determines which hands to include and not.</param>
    /// <returns>The probability of drawing all hands that match <paramref name="filter"/>.</returns>
    [Pure]
    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, GetCombinations(), handAnalyzer.DeckSize, handAnalyzer.HandSize);

        IEnumerable<HandCombination<TCardGroupName>> GetCombinations()
        {
            foreach (var hand in handAnalyzer.Combinations)
            {
                if (filter(handAnalyzer, hand))
                {
                    yield return hand;
                }
            }
        }
    }

    /// <summary>
    /// <para>Calculates the probability of drawing all hands that match <paramref name="filter"/>.</para>
    /// <para>You can also access <paramref name="handAnalyzer"/> and <paramref name="args"/> in <paramref name="filter"/>.</para>
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <typeparam name="TArgs">The type of the data being passed to <paramref name="filter"/>.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="args">The data being passed to <paramref name="filter"/>.</param>
    /// <param name="filter">The filter, which determines which hands to include and not.</param>
    /// <returns>The probability of drawing all hands that match <paramref name="filter"/>.</returns>
    [Pure]
    public static double CalculateProbability<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TArgs, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, GetCombinations(), handAnalyzer.DeckSize, handAnalyzer.HandSize);

        IEnumerable<HandCombination<TCardGroupName>> GetCombinations()
        {
            foreach (var hand in handAnalyzer.Combinations)
            {
                if (filter(handAnalyzer, hand, args))
                {
                    yield return hand;
                }
            }
        }
    }

    /// <summary>
    /// Calculates the probability of drawing all hands that match <paramref name="filter"/>.
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="filter">The filter, which determines which hands to include and not.</param>
    /// <returns>The probability of drawing all hands that match <paramref name="filter"/>.</returns>
    [Pure]
    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, IFilter<HandCombination<TCardGroupName>> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, filter.GetResults(handAnalyzer.Combinations), handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    /// <summary>
    /// Calculates the Expected Value (EV) of all hands. For each hand, take the return value multiplied by the probability of drawing that hand.
    /// The return value of this function is the summation of each hand multiplied by its draw probability.
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="valueSelector">The return value for each hand. Each value is multiplied by the probability of drawing that hand.</param>
    /// <returns>The summation of each return value from <paramref name="valueSelector"/> applied to each hand multiplied by the probability of drawing that hand.</returns>
    [Pure]
    public static double CalculateExpectedValue<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandCombination<TCardGroupName>, double> valueSelector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
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
    /// <para>Calculates the Expected Value (EV) of all hands. For each hand, take the return value multiplied by the probability of drawing that hand.
    /// The return value of this function is the summation of each hand multiplied by its draw probability.</para>
    /// <para>You can also access <paramref name="args"/> in <paramref name="valueSelector"/>.</para>
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <typeparam name="TArgs">The data type being used in <paramref name="valueSelector"/>.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="args">Any relevant data for <paramref name="valueSelector"/>.</param>
    /// <param name="valueSelector">The return value for each hand. Each value is multiplied by the probability of drawing that hand.</param>
    /// <returns>The summation of each return value from <paramref name="valueSelector"/> applied to each hand multiplied by the probability of drawing that hand.</returns>
    [Pure]
    public static double CalculateExpectedValue<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandCombination<TCardGroupName>, TArgs, double> valueSelector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
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
    /// <para>Calculates the Expected Value (EV) of all hands. For each hand, take the return value multiplied by the probability of drawing that hand.
    /// The return value of this function is the summation of each hand multiplied by its draw probability.</para>
    /// <para>You can also access <paramref name="handAnalyzer"/> in <paramref name="valueSelector"/>.</para>
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="valueSelector">The return value for each hand. Each value is multiplied by the probability of drawing that hand.</param>
    /// <returns>The summation of each return value from <paramref name="valueSelector"/> applied to each hand multiplied by the probability of drawing that hand.</returns>
    [Pure]
    public static double CalculateExpectedValue<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, double> valueSelector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
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
    /// <para>Calculates the Expected Value (EV) of all hands. For each hand, take the return value multiplied by the probability of drawing that hand.
    /// The return value of this function is the summation of each hand multiplied by its draw probability.</para>
    /// <para>You can also access <paramref name="handAnalyzer"/> and <paramref name="args"/> in <paramref name="valueSelector"/>.</para>
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <typeparam name="TArgs">The data type being used in <paramref name="valueSelector"/>.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="args">Any relevant data for <paramref name="valueSelector"/>.</param>
    /// <param name="valueSelector">The return value for each hand. Each value is multiplied by the probability of drawing that hand.</param>
    /// <returns>The summation of each return value from <paramref name="valueSelector"/> applied to each hand multiplied by the probability of drawing that hand.</returns>
    [Pure]
    public static double CalculateExpectedValue<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TArgs, double> valueSelector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
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
    /// Calculates the Expected Value (EV) of a subset of hands. The EV is sum of each hand's probability multiplied by some value produced by that hand.
    /// </summary>
    /// <remarks>
    /// <para>For example, each hand has a certain number of Trap Cards (zero is valid). To find the EV, we count the number of Trap Cards in a hand (T_x),
    /// then multiple that by the probability of drawing that hand (P_x). We do that for every hand (x), and the summation of all those (T_x * P_x) is the EV.</para>
    /// <para>This method calculates the EV of a subset of hands. We apply a filter called F, which determines which hands to include in the subset and which not.
    /// If A is the set of all hands and EV(A) is its expected value, then B = F(A) and its expected value is EV(B) = EV(F(A)).</para>
    /// <para>For example, we play a trap deck with "Wannabee!" in it and we want to know how many traps we will have if we draw "Wannabee!" EV(A) would be the EV
    /// of all possible hands, while EV(B) would be the EV of only hands where we do draw "Wannabee!"</para>
    /// </remarks>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="filter">The filter F. This creates subset B of A, B = F(A).</param>
    /// <param name="valueSelector">The value corresponding to the hand. This can be the number of trap cards, the number of starters, etc. This get multiplied by the probability of drawing the hand.</param>
    /// <returns>EV(B), B = F(A).</returns>
    [Pure]
    public static double CalculateExpectedValueOfSubset<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandCombination<TCardGroupName>, bool> filter, Func<HandCombination<TCardGroupName>, double> valueSelector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var ev = 0.0;
        var totalProb = 0.0;

        foreach (var hand in handAnalyzer.Combinations)
        {
            if (!filter(hand))
            {
                continue;
            }

            var numberOfTraps = valueSelector(hand);
            var prob = handAnalyzer.CalculateProbability(hand);
            totalProb += prob;
            ev += prob * numberOfTraps;
        }

        return ev / totalProb;
    }

    /// <summary>
    /// <para>Calculates the Expected Value (EV) of a subset of hands. The EV is sum of each hand's probability multiplied by some value produced by that hand.</para>
    /// </summary>
    /// <remarks>
    /// <para>For example, each hand has a certain number of Trap Cards (zero is valid). To find the EV, we count the number of Trap Cards in a hand (T_x),
    /// then multiple that by the probability of drawing that hand (P_x). We do that for every hand (x), and the summation of all those (T_x * P_x) is the EV.</para>
    /// <para>This method calculates the EV of a subset of hands. We apply a filter called F, which determines which hands to include in the subset and which not.
    /// If A is the set of all hands and EV(A) is its expected value, then B = F(A) and its expected value is EV(B) = EV(F(A)).</para>
    /// <para>For example, we play a trap deck with "Wannabee!" in it and we want to know how many traps we will have if we draw "Wannabee!" EV(A) would be the EV
    /// of all possible hands, while EV(B) would be the EV of only hands where we do draw "Wannabee!"</para>
    /// <para>You can also access <paramref name="args"/> in <paramref name="filter"/> and <paramref name="valueSelector"/>.</para>
    /// </remarks>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <typeparam name="TArgs">The type of <paramref name="args"/>.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="args">Any relevant for <paramref name="filter"/> and <paramref name="valueSelector"/>.</param>
    /// <param name="filter">The filter F. This creates subset B of A, B = F(A).</param>
    /// <param name="valueSelector">The value corresponding to the hand. This can be the number of trap cards, the number of starters, etc. This get multiplied by the probability of drawing the hand.</param>
    /// <returns>EV(B), B = F(A).</returns>
    [Pure]
    public static double CalculateExpectedValueOfSubset<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandCombination<TCardGroupName>, TArgs, bool> filter, Func<HandCombination<TCardGroupName>, TArgs, double> valueSelector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var ev = 0.0;
        var totalProb = 0.0;

        foreach (var hand in handAnalyzer.Combinations)
        {
            if (!filter(hand, args))
            {
                continue;
            }

            var numberOfTraps = valueSelector(hand, args);
            var prob = handAnalyzer.CalculateProbability(hand);
            totalProb += prob;
            ev += prob * numberOfTraps;
        }

        return ev / totalProb;
    }

    /// <summary>
    /// <para>Calculates the Expected Value (EV) of a subset of hands. The EV is sum of each hand's probability multiplied by some value produced by that hand.</para>
    /// </summary>
    /// <remarks>
    /// <para>For example, each hand has a certain number of Trap Cards (zero is valid). To find the EV, we count the number of Trap Cards in a hand (T_x),
    /// then multiple that by the probability of drawing that hand (P_x). We do that for every hand (x), and the summation of all those (T_x * P_x) is the EV.</para>
    /// <para>This method calculates the EV of a subset of hands. We apply a filter called F, which determines which hands to include in the subset and which not.
    /// If A is the set of all hands and EV(A) is its expected value, then B = F(A) and its expected value is EV(B) = EV(F(A)).</para>
    /// <para>For example, we play a trap deck with "Wannabee!" in it and we want to know how many traps we will have if we draw "Wannabee!" EV(A) would be the EV
    /// of all possible hands, while EV(B) would be the EV of only hands where we do draw "Wannabee!"</para>
    /// <para>You can also access <paramref name="handAnalyzer"/> in <paramref name="filter"/> and <paramref name="valueSelector"/>.</para>
    /// </remarks>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="filter">The filter F. This creates subset B of A, B = F(A).</param>
    /// <param name="valueSelector">The value corresponding to the hand. This can be the number of trap cards, the number of starters, etc. This get multiplied by the probability of drawing the hand.</param>
    /// <returns>EV(B), B = F(A).</returns>
    [Pure]
    public static double CalculateExpectedValueOfSubset<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, bool> filter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, double> valueSelector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var ev = 0.0;
        var totalProb = 0.0;

        foreach (var hand in handAnalyzer.Combinations)
        {
            if (!filter(handAnalyzer, hand))
            {
                continue;
            }

            var numberOfTraps = valueSelector(handAnalyzer, hand);
            var prob = handAnalyzer.CalculateProbability(hand);
            totalProb += prob;
            ev += prob * numberOfTraps;
        }

        return ev / totalProb;
    }

    /// <summary>
    /// <para>Calculates the Expected Value (EV) of a subset of hands. The EV is sum of each hand's probability multiplied by some value produced by that hand.</para>
    /// </summary>
    /// <remarks>
    /// <para>For example, each hand has a certain number of Trap Cards (zero is valid). To find the EV, we count the number of Trap Cards in a hand (T_x),
    /// then multiple that by the probability of drawing that hand (P_x). We do that for every hand (x), and the summation of all those (T_x * P_x) is the EV.</para>
    /// <para>This method calculates the EV of a subset of hands. We apply a filter called F, which determines which hands to include in the subset and which not.
    /// If A is the set of all hands and EV(A) is its expected value, then B = F(A) and its expected value is EV(B) = EV(F(A)).</para>
    /// <para>For example, we play a trap deck with "Wannabee!" in it and we want to know how many traps we will have if we draw "Wannabee!" EV(A) would be the EV
    /// of all possible hands, while EV(B) would be the EV of only hands where we do draw "Wannabee!"</para>
    /// <para>You can also access <paramref name="handAnalyzer"/> and <paramref name="args"/> in <paramref name="filter"/> and <paramref name="valueSelector"/>.</para>
    /// </remarks>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <typeparam name="TArgs">The type of <paramref name="args"/>.</typeparam>
    /// <param name="handAnalyzer">The hand analyzer.</param>
    /// <param name="args">Any relevant for <paramref name="filter"/> and <paramref name="valueSelector"/>.</param>
    /// <param name="filter">The filter F. This creates subset B of A, B = F(A).</param>
    /// <param name="valueSelector">The value corresponding to the hand. This can be the number of trap cards, the number of starters, etc. This get multiplied by the probability of drawing the hand.</param>
    /// <returns>EV(B), B = F(A).</returns>
    [Pure]
    public static double CalculateExpectedValueOfSubset<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TArgs, bool> filter, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TArgs, double> valueSelector)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var ev = 0.0;
        var totalProb = 0.0;

        foreach (var hand in handAnalyzer.Combinations)
        {
            if (!filter(handAnalyzer, hand, args))
            {
                continue;
            }

            var numberOfTraps = valueSelector(handAnalyzer, hand, args);
            var prob = handAnalyzer.CalculateProbability(hand);
            totalProb += prob;
            ev += prob * numberOfTraps;
        }

        return ev / totalProb;
    }

    [Pure]
    public static double Aggregate<TCardGroup, TCardGroupName, TAggregate>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandCombination<TCardGroupName>, TAggregate> aggregator, Func<IReadOnlyDictionary<HandCombination<TCardGroupName>, TAggregate>, double> calculator)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var aggregateValues = new Dictionary<HandCombination<TCardGroupName>, TAggregate>(handAnalyzer.Combinations.Count);

        foreach (var hand in handAnalyzer.Combinations)
        {
            aggregateValues[hand] = aggregator(hand);
        }

        return calculator(aggregateValues);
    }

    [Pure]
    public static double Aggregate<TCardGroup, TCardGroupName, TAggregate>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAggregate> aggregator, Func<IReadOnlyDictionary<HandCombination<TCardGroupName>, TAggregate>, double> calculator)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var aggregateValues = new Dictionary<HandCombination<TCardGroupName>, TAggregate>(handAnalyzer.Combinations.Count);

        foreach (var hand in handAnalyzer.Combinations)
        {
            aggregateValues[hand] = aggregator(hand, handAnalyzer);
        }

        return calculator(aggregateValues);
    }

    public static CardList<CardGroup<TCardGroupName>, TCardGroupName> ConvertToCardGroup<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return CardList
            .Create(handAnalyzer)
            .Cast(CardGroup.CreateFrom<TCardGroup, TCardGroupName>);
    }
}

public sealed class HandAnalyzer<TCardGroup, TCardGroupName> : IDataComparisonFormatterEntry, ICalculator<HandAnalyzer<TCardGroup, TCardGroupName>>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private readonly TCardGroup[] _cardGroups;

    public int DeckSize { get; }
    public int HandSize { get; }

    public IReadOnlyDictionary<TCardGroupName, TCardGroup> CardGroups { get; }
    public IReadOnlyList<TCardGroupName> CardNames { get; }
    public IReadOnlySet<HandCombination<TCardGroupName>> Combinations { get; }
    public string AnalyzerName { get; }

    public HandAnalyzer(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> args)
    {
        AnalyzerName = args.AnalyzerName;
        HandSize = args.HandSize;

        _cardGroups = [..args.CardGroups];
        CardNames = [.. _cardGroups.Select(static card => card.Name)];
        CardGroups = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, _cardGroups);

        DeckSize = CardGroups.Values.Sum(static group => group.Size);
        Guard.IsGreaterThanOrEqualTo(DeckSize, 1, nameof(DeckSize));
        Guard.IsLessThanOrEqualTo(DeckSize, 60, nameof(DeckSize));

        Span<byte> currentHandCounts = stackalloc byte[_cardGroups.Length];
        var hands = new List<HandCombination<TCardGroupName>>();
        Recurse(currentHandCounts, 0, HandSize, hands);
        Combinations = hands.ToHashSet();
    }

    public double CalculateProbability(Func<HandCombination<TCardGroupName>, bool> filter)
    {
        double prob = 0.0;
        foreach (var hand in Combinations)
        {
            if (filter(hand))
            {
                prob += CalculateProbability(hand);
            }
        }
        return prob;
    }

    public double CalculateProbability(HandCombination<TCardGroupName> currentHand)
    {
        return CalculateProbability(currentHand.Hand.Span);
    }

    private void Recurse(Span<byte> currentHandCounts, int groupIndex, int cardsNeeded, List<HandCombination<TCardGroupName>> hands)
    {
        if (cardsNeeded == 0)
        {
            hands.Add(new HandCombination<TCardGroupName>(currentHandCounts.ToArray(), CardNames));
            return;
        }

        if (groupIndex >= _cardGroups.Length) return;

        var group = _cardGroups[groupIndex];

        int maxTake = Math.Min(group.Size, cardsNeeded);

        for (int count = maxTake; count >= 0; count--)
        {
            currentHandCounts[groupIndex] = (byte)count;
            Recurse(currentHandCounts, groupIndex + 1, cardsNeeded - count, hands);
        }

        currentHandCounts[groupIndex] = 0;
    }

    private double CalculateProbability(ReadOnlySpan<byte> currentHand)
    {
        long numerator = 1;

        for (int i = 0; i < currentHand.Length; i++)
        {
            int k = currentHand[i];
            if (k == 0) continue;

            int n = _cardGroups[i].Size;
            numerator *= BinomialCache.Choose(n, k);
        }

        return numerator / (double)BinomialCache.Choose(DeckSize, HandSize);
    }

    public string GetHeader()
    {
        return $"{AnalyzerName} ({HandSize:N0})";
    }

    public string GetDescription()
    {
        return $"Analyzer: {AnalyzerName}. Cards: {DeckSize:N0}. Hand Size: {HandSize:N0}. Possible Hands: {Combinations.Count:N0}.";
    }

    double ICalculator<HandAnalyzer<TCardGroup, TCardGroupName>>.Calculate(Func<HandAnalyzer<TCardGroup, TCardGroupName>, double> selector)
    {
        return selector(this);
    }

    double ICalculator<HandAnalyzer<TCardGroup, TCardGroupName>>.Calculate<TArgs>(TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, double> selector)
    {
        return selector(this, args);
    }

    IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> ICalculator<HandAnalyzer<TCardGroup, TCardGroupName>>.Map<TReturn>(Func<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> selector)
    {
        return new Dictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn>
        {
            [this] = selector(this)
        };
    }
}
