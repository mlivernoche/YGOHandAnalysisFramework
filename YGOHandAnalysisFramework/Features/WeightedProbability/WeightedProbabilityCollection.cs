﻿using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Features.WeightedProbability;

public static class WeightedProbabilityCollection
{
    public static WeightedProbabilityCollection<TWeighted> CreateWithEqualWeights<TWeighted>(string name, IEnumerable<TWeighted> weighteds)
        where TWeighted : notnull
    {
        return new WeightedProbabilityCollection<TWeighted>(name, weighteds.Select(static weighted => new WeightedData<TWeighted>(1.0, weighted)));
    }
}

public record WeightedProbabilityCollection<TWeighted> : IDataComparisonFormatterEntry, ICalculator<TWeighted>
    where TWeighted : notnull
{
    private string Name { get; }
    private HashSet<WeightedData<TWeighted>> WeightedData { get; }
    private double TotalWeights { get; }

    public WeightedProbabilityCollection(string name, IEnumerable<WeightedData<TWeighted>> weighted)
    {
        Name = name;
        WeightedData = new HashSet<WeightedData<TWeighted>>(weighted);
        TotalWeights = WeightedData.Sum(static x => x.Weight);
    }

    public double Calculate(Func<TWeighted, double> selector)
    {
        var probability = 0.0;

        foreach(var data in WeightedData)
        {
            probability += data.Calculate(TotalWeights, selector);
        }

        return probability;
    }

    public double Calculate<TArgs>(TArgs args, Func<TWeighted, TArgs, double> selector)
    {
        var probability = 0.0;

        foreach (var data in WeightedData)
        {
            probability += data.Calculate(TotalWeights, args, selector);
        }

        return probability;
    }

    public IReadOnlyDictionary<TWeighted, TReturn> Map<TReturn>(Func<TWeighted, TReturn> selector)
    {
        var dict = new Dictionary<TWeighted, TReturn>();

        foreach(var weighted in WeightedData)
        {
            dict[weighted.WeightedValue] = selector(weighted.WeightedValue);
        }

        return dict;
    }

    public string GetHeader()
    {
        return Name;
    }

    public string GetDescription()
    {
        return Name;
    }
}
