using System.Numerics;

namespace YGOHandAnalysisFramework.Features.Comparison.Comparer
{
    internal class DescendingComparer<T> : IComparer<T> where T : notnull, INumber<T>
    {
        internal static IComparer<T> Default { get; } = new DescendingComparer<T>();

        public int Compare(T? x, T? y)
        {
            if (x != null)
            {
                if (y != null)
                {
                    return y.CompareTo(x);
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
