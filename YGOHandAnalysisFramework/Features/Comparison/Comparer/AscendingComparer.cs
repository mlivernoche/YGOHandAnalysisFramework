using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace YGOHandAnalysisFramework.Features.Comparison.Comparer
{
    internal class AscendingComparer<T> : IComparer<T> where T : notnull, INumber<T>
    {
        internal static IComparer<T> Default { get; } = new AscendingComparer<T>();

        public int Compare(T? x, T? y)
        {
            if (x != null)
            {
                if (y != null)
                {
                    return x.CompareTo(y);
                }

                return 1;
            }

            if (y != null)
            {
                return -1;
            }

            return 0;
        }
    }
}
