using System.Collections;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Features.Comparison.Calculator;

public class CalculatorWrapperCollection<T> : IReadOnlyCollection<ICalculatorWrapper<T>>
    where T : ICalculator<T>, IDataComparisonFormatterEntry
{
    public int Count => Wrappers.Count;

    private List<ICalculatorWrapper<T>> Wrappers { get; } = new List<ICalculatorWrapper<T>>();

    public void Add<TValue>(TValue value)
        where TValue : ICalculator<T>, IDataComparisonFormatterEntry
    {
        Wrappers.Add(new CalculatorWrapper<T, TValue>(value));
    }

    public IEnumerator<ICalculatorWrapper<T>> GetEnumerator()
    {
        IEnumerable<ICalculatorWrapper<T>> enumerable = Wrappers;
        return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerable = Wrappers;
        return enumerable.GetEnumerator();
    }
}
