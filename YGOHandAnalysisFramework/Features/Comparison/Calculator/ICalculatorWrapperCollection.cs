using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Features.Comparison.Calculator;

public interface ICalculatorWrapperCollection<T> : IReadOnlyCollection<ICalculatorWrapper<T>>
    where T : ICalculator<T>, IDataComparisonFormatterEntry
{

}
