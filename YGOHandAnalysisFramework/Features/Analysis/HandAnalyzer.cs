using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Probability;
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
