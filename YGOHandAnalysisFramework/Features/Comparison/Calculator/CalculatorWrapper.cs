using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Features.Comparison.Calculator;

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
