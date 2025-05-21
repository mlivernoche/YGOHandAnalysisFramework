using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Features.Comparison.Calculator;

public static class CalculatorWrapper
{
    public static Func<ICalculatorWrapper<TWrapped>, double> Wrap<TWrapped>(this Func<TWrapped, double> func)
    {
        return calculator => calculator.Calculate(func);
    }

    public static Func<ICalculatorWrapper<TWrapped>, double> Wrap<TWrapped, TArgs>(this Func<TWrapped, TArgs, double> func, TArgs args)
    {
        return calculator => calculator.Calculate(args, func);
    }

    public static IReadOnlyDictionary<TWrapped, TReturn> Map<TWrapped, TReturn>(this ICalculatorWrapperCollection<TWrapped> calculators, Func<TWrapped, TReturn> selector)
        where TWrapped : ICalculator<TWrapped>, IDataComparisonFormatterEntry
    {
        var dict = new Dictionary<TWrapped, TReturn>();

        foreach (var calculator in calculators)
        {
            var map = calculator.Map(selector);

            foreach(var (wrapped, ret) in map)
            {
                dict[wrapped] = ret;
            }
        }

        return dict;
    }

    public static int GetMaxHandSize<TCardGroup, TCardGroupName>(this ICalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>> calculators)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return calculators
            .Map(static handAnalyzer => handAnalyzer.HandSize)
            .Max(static kv => kv.Value);
    }
}

public class CalculatorWrapper<TWrapped, TValue> : ICalculatorWrapper<TWrapped>
    where TWrapped : notnull, ICalculator<TWrapped>, IDataComparisonFormatterEntry
    where TValue : notnull, ICalculator<TWrapped>, IDataComparisonFormatterEntry
{
    private TValue Value { get; }

    public CalculatorWrapper(TValue value)
    {
        Value = value;
    }

    public double Calculate(Func<TWrapped, double> selector)
    {
        ICalculator<TWrapped> calculator = Value;
        return calculator.Calculate(selector);
    }

    public double Calculate<TArgs>(TArgs args, Func<TWrapped, TArgs, double> selector)
    {
        ICalculator<TWrapped> calculator = Value;
        return calculator.Calculate(args, selector);
    }

    public IReadOnlyDictionary<TWrapped, TReturn> Map<TReturn>(Func<TWrapped, TReturn> selector)
    {
        ICalculator<TWrapped> calculator = Value;
        return calculator.Map(selector);
    }

    public string GetHeader()
    {
        IDataComparisonFormatterEntry entry = Value;
        return entry.GetHeader();
    }

    public string GetDescription()
    {
        IDataComparisonFormatterEntry entry = Value;
        return entry.GetDescription();
    }
}

public class CalculatorWrapper<TWrapped>(TWrapped value) : CalculatorWrapper<TWrapped, TWrapped>(value)
    where TWrapped : notnull, ICalculator<TWrapped>, IDataComparisonFormatterEntry;
