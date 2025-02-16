using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Features.Probability;

public interface ICalculator<T>
{
    double Calculate(Func<T, double> selector);
    double Calculate<TArgs>(TArgs args, Func<T, TArgs, double> selector);
}
