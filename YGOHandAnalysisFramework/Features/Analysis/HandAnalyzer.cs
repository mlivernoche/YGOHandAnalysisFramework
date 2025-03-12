using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Probability;
using YGOHandAnalysisFramework.Features.Assessment;
using YGOHandAnalysisFramework.Features.Caching;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Analysis;

public static class HandAnalyzer
{
    public static HandAnalyzer<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArguments)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzer<TCardGroup, TCardGroupName>(buildArguments);
    }

    public static HandAnalyzer<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> buildArguments, HandAnalyzerLoader<TCardGroup, TCardGroupName> loader)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if (loader.TryLoadHandAnalyzer(buildArguments).GetResult(out var loadedAnalyzer, out _))
        {
            return loadedAnalyzer;
        }

        var handAnalyzer = Create(buildArguments);
        loader.CreateCache(handAnalyzer);

        return handAnalyzer;
    }

    public static IReadOnlyDictionary<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>> CreateInParallel<TCardGroup, TCardGroupName>(IEnumerable<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>> buildArguments, HandAnalyzerLoader<TCardGroup, TCardGroupName> loader)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var hardLoads = new List<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>>();
        var analyzers = new Dictionary<HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>, HandAnalyzer <TCardGroup, TCardGroupName>>();

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

        foreach(var (args, analyzer) in hardLoadedAnalyzers)
        {
            analyzers[args] = analyzer;
            loader.CreateCache(analyzer);
        }

        return analyzers;
    }

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

        foreach(var (buildArgs, analyzer) in analyzers)
        {
            analyzerByBuildArgs[buildArgs] = analyzer;
        }

        return analyzerByBuildArgs;
    }

    public static HandAnalyzer<TCardGroup, TCardGroupName> Excavate<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> originalAnalyzer, HandCombination<TCardGroupName> hand)
        where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardList = originalAnalyzer.CardGroups.Values.RemoveHand(hand);
        var handSize = Math.Min(cardList.GetNumberOfCards(), originalAnalyzer.HandSize);

        Guard.IsGreaterThan(handSize, 0);

        var args = HandAnalyzerBuildArguments.Create(originalAnalyzer.AnalyzerName, handSize, cardList);
        return Create(args);
    }

    public static HandAnalyzer<TCardGroup, TCardGroupName> Excavate<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> originalAnalyzer, HandCombination<TCardGroupName> hand, int handSize)
        where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardList = originalAnalyzer.CardGroups.Values.RemoveHand(hand);
        var actualHandSize = Math.Min(cardList.GetNumberOfCards(), handSize);

        Guard.IsGreaterThan(actualHandSize, 0);
        var args = HandAnalyzerBuildArguments.Create(originalAnalyzer.AnalyzerName, actualHandSize, cardList);
        return Create(args);
    }

    private static IEnumerable<HandCombination<TCardGroupName>> Filter<TCardGroup, TCardGroupName>(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var hand in analyzer.Combinations)
        {
            if (filter(analyzer, hand))
            {
                yield return hand;
            }
        }
    }

    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, Filter(handAnalyzer, filter), handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    public static double CalculateProbability<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TArgs, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
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

        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, GetCombinations(), handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    public static double CalculateProbability<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandCombination<TCardGroupName>, TArgs, bool> filter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
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

        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, GetCombinations(), handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    public static double CalculateProbability<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, HandCombination<TCardGroupName> hand)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return Calculator.CalculateProbability(handAnalyzer.CardGroups.Values, hand, handAnalyzer.DeckSize, handAnalyzer.HandSize);
    }

    public static double CalculateExpectedValue<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandCombination<TCardGroupName>, TArgs, double> valueFunction)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var expectedValue = 0.0;

        foreach (var hand in handAnalyzer.Combinations)
        {
            var value = valueFunction(hand, args);
            if (value > 0)
            {
                expectedValue += handAnalyzer.CalculateProbability(hand) * value;
            }
        }

        return expectedValue;
    }

    public static double CalculateExpectedValue<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandCombination<TCardGroupName>, double> valueFunction)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var expectedValue = 0.0;

        foreach (var hand in handAnalyzer.Combinations)
        {
            var value = valueFunction(hand);
            if (value > 0)
            {
                expectedValue += handAnalyzer.CalculateProbability(hand) * value;
            }
        }

        return expectedValue;
    }

    public static double CalculateExpectedValue<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, double> valueFunction)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var expectedValue = 0.0;

        foreach (var hand in handAnalyzer.Combinations)
        {
            var value = valueFunction(handAnalyzer, hand);
            if (value > 0)
            {
                expectedValue += handAnalyzer.CalculateProbability(hand) * value;
            }
        }

        return expectedValue;
    }

    /// <summary>
    /// Calculates the Expected Value (EV) of all hands. For each hand, take
    /// the return value multiplied by the probability of drawing that hand. The return
    /// value of this function is the summation of each hand multiplied by its draw probability.
    /// </summary>
    /// <typeparam name="TCardGroup">The card group type, which has all the data for that card (name, amount, stats, etc.)</typeparam>
    /// <typeparam name="TCardGroupName">The card name type.</typeparam>
    /// <typeparam name="TArgs">The data type being used in the valueFunction.</typeparam>
    /// <param name="handAnalyzer">The HandAnalyzer.</param>
    /// <param name="args">Any relevant for the valueFunction.</param>
    /// <param name="valueFunction">The return value for each hand. Each value is multiplied by the probability of drawing that hand.</param>
    /// <returns>The summation of each return value from valueFunction applied to each hand multipled by the probability of drawing that hand.</returns>
    public static double CalculateExpectedValue<TCardGroup, TCardGroupName, TArgs>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, HandCombination<TCardGroupName>, double> valueFunction)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var expectedValue = 0.0;

        foreach (var hand in handAnalyzer.Combinations)
        {
            var value = valueFunction(handAnalyzer, args, hand);
            if (value > 0)
            {
                expectedValue += handAnalyzer.CalculateProbability(hand) * value;
            }
        }

        return expectedValue;
    }

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
}

public sealed class HandAnalyzer<TCardGroup, TCardGroupName> : IDataComparisonFormatterEntry, ICalculator<HandAnalyzer<TCardGroup, TCardGroupName>>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public int DeckSize { get; }
    public int HandSize { get; }
    public IReadOnlyDictionary<TCardGroupName, TCardGroup> CardGroups { get; }
    public IImmutableSet<HandCombination<TCardGroupName>> Combinations { get; }
    public string AnalyzerName { get; }

    public HandAnalyzer(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> args)
    {
        AnalyzerName = args.AnalyzerName;
        HandSize = args.HandSize;

        CardGroups = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, args.CardGroups);

        DeckSize = CardGroups.Values.Sum(static group => group.Size);
        Guard.IsGreaterThanOrEqualTo(DeckSize, 1, nameof(DeckSize));
        Guard.IsLessThanOrEqualTo(DeckSize, 60, nameof(DeckSize));

        Combinations = HandCombinationFinder.GetCombinations<TCardGroup, TCardGroupName>(args.HandSize, CardGroups.Values);
    }

    public HandAnalyzer(HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> args, IEnumerable<HandCombination<TCardGroupName>> handCombinations)
    {
        AnalyzerName = args.AnalyzerName;
        HandSize = args.HandSize;

        CardGroups = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, args.CardGroups);

        DeckSize = CardGroups.Values.Sum(static group => group.Size);
        Guard.IsGreaterThanOrEqualTo(DeckSize, 1, nameof(DeckSize));
        Guard.IsLessThanOrEqualTo(DeckSize, 60, nameof(DeckSize));

        Combinations = handCombinations.ToImmutableHashSet();
    }

    public double CalculateProbability()
    {
        return Calculator.CalculateProbability(CardGroups.Values, Combinations, DeckSize, HandSize);
    }

    public double CalculateProbability(Func<HandCombination<TCardGroupName>, bool> filter)
    {
        return Calculator.CalculateProbability(CardGroups.Values, Combinations.Where(filter), DeckSize, HandSize);
    }

    public double CalculateProbability(IFilter<HandCombination<TCardGroupName>> filter)
    {
        return Calculator.CalculateProbability(CardGroups.Values, filter.GetResults(Combinations), DeckSize, HandSize);
    }

    public double CalculateProbability<TFilter>(TFilter filter)
        where TFilter : struct, IFilter<HandCombination<TCardGroupName>>
    {
        return Calculator.CalculateProbability(CardGroups.Values, filter.GetResults(Combinations), DeckSize, HandSize);
    }

    public HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> AssessHands<TAssessment>(Func<HandCombination<TCardGroupName>, TAssessment> filter)
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var assessments = Combinations.Select(filter).ToList();
        var includedHands = assessments
            .Where(static assessment => assessment.Included)
            .Select(static assessment => assessment.Hand);
        var prob = Calculator.CalculateProbability(CardGroups.Values, includedHands, DeckSize, HandSize);

        return new HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>(this, prob, assessments);
    }

    public HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> AssessHands<TAssessment>(Func<HandCombination<TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>, TAssessment> filter)
        where TAssessment : IHandAssessment<TCardGroupName>
    {
        var assessments = new List<TAssessment>();

        foreach (var hand in Combinations)
        {
            assessments.Add(filter(hand, this));
        }

        var includedHands = assessments
            .Where(static assessment => assessment.Included)
            .Select(static assessment => assessment.Hand);
        var prob = Calculator.CalculateProbability(CardGroups.Values, includedHands, DeckSize, HandSize);

        return new HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>(this, prob, assessments);
    }

    public int CountHands(Func<HandCombination<TCardGroupName>, bool> filter)
    {
        return Combinations.Count(filter);
    }

    public int[] CountUniqueCardName()
    {
        var counts = new int[HandSize + 1];

        foreach (var combination in Combinations)
        {
            var count = combination.CountCardNames();
            counts[count]++;
        }

        return counts;
    }

    public string GetHeader()
    {
        return $"{AnalyzerName} ({HandSize:N0})";
    }

    public string GetDescription()
    {
        return $"Analyzer: {AnalyzerName}. Cards: {DeckSize:N0}. Hand Size: {HandSize:N0}. Possible Hands: {Combinations.Count:N0}.";
    }

    public double Calculate(Func<HandAnalyzer<TCardGroup, TCardGroupName>, double> selector)
    {
        return selector(this);
    }

    public double Calculate<TArgs>(TArgs args, Func<HandAnalyzer<TCardGroup, TCardGroupName>, TArgs, double> selector)
    {
        return selector(this, args);
    }
}
