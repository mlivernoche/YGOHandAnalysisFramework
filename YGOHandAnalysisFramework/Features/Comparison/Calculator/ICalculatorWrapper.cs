using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Probability;

namespace YGOHandAnalysisFramework.Features.Comparison.Calculator;

public interface ICalculatorWrapper<TWrapped> : ICalculator<TWrapped>, IDataComparisonFormatterEntry
{
}
