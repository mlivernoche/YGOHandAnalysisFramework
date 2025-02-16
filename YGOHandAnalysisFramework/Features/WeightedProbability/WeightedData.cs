using CommunityToolkit.Diagnostics;

namespace YGOHandAnalysisFramework.Features.WeightedProbability;

public static class WeightedData
{
    public static WeightedData<TWeightedValue> Create<TWeightedValue>(double weight, TWeightedValue weightedValue)
    {
        return new WeightedData<TWeightedValue>(weight, weightedValue);
    }
}

public record WeightedData<TWeightedValue>(double Weight, TWeightedValue WeightedValue)
{
    public double Calculate(double totalWeight, Func<TWeightedValue, double> selector)
    {
        Guard.IsGreaterThan(totalWeight, 0);
        Guard.IsGreaterThanOrEqualTo(totalWeight, Weight);
        Guard.IsGreaterThanOrEqualTo(Weight, 0);

        var probability = Weight / totalWeight;
        return probability * selector(WeightedValue);
    }

    public double Calculate<TArgs>(double totalWeight, TArgs args, Func<TWeightedValue, TArgs, double> selector)
    {
        Guard.IsGreaterThan(totalWeight, 0);
        Guard.IsGreaterThanOrEqualTo(totalWeight, Weight);
        Guard.IsGreaterThanOrEqualTo(Weight, 0);

        var probability = Weight / totalWeight;
        return probability * selector(WeightedValue, args);
    }
}
