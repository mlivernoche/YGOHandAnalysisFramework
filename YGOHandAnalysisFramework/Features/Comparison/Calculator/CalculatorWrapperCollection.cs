using System.Collections;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Features.Comparison.Calculator;

public class CalculatorWrapperCollection<T> : ICalculatorWrapperCollection<T>
    where T : ICalculator<T>, IDataComparisonFormatterEntry
{
    public int Count => Wrappers.Count;

    private List<ICalculatorWrapper<T>> Wrappers { get; } = new List<ICalculatorWrapper<T>>();

    public CalculatorWrapperCollection() { }

    public CalculatorWrapperCollection(IEnumerable<T> calculators)
    {
        foreach(var calculator in calculators)
        {
            Add(calculator);
        }
    }

    public CalculatorWrapperCollection(IEnumerable<ICalculatorWrapper<T>> calculators)
    {
        Wrappers.AddRange(calculators);
    }

    public void Add<TValue>(TValue value)
        where TValue : ICalculator<T>, IDataComparisonFormatterEntry
    {
        Wrappers.Add(new CalculatorWrapper<T, TValue>(value));
    }

    public void Add(T value)
    {
        Wrappers.Add(new CalculatorWrapper<T>(value));
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
