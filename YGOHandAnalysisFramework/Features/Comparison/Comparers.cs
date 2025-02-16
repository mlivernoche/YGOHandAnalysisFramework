using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YGOHandAnalysisFramework.Features.Comparison.Comparer;

namespace YGOHandAnalysisFramework.Features.Comparison;

public static partial class HandAnalyzerComparison
{
    public static IComparer<T> GetAscendingComparer<T>()
        where T : notnull, INumber<T>
    {
        return AscendingComparer<T>.Default;
    }

    public static IComparer<T> GetDescendingComparer<T>()
        where T : notnull, INumber<T>
    {
        return DescendingComparer<T>.Default;
    }
}
