namespace YGOHandAnalysisFramework;

internal static class BinomialCache
{
    // 70 is enough for a 60-card deck + Extra Deck, etc.
    public const int MaxN = 60;
    private static readonly long[,] _table;

    static BinomialCache()
    {
        _table = new long[MaxN + 1, MaxN + 1];

        for (int n = 0; n <= MaxN; n++)
        {
            _table[n, 0] = 1;
            for (int k = 1; k <= n; k++)
            {
                // Pascal's Identity: nCk = (n-1)C(k-1) + (n-1)Ck
                // Uses 'checked' to throw if we somehow overflow long (unlikely for deck sizes)
                _table[n, k] = checked(_table[n - 1, k - 1] + _table[n - 1, k]);
            }
        }
    }

    // O(1) Lookup
    public static long Choose(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        if (n > MaxN) throw new ArgumentOutOfRangeException($"n cannot exceed {MaxN}");
        return _table[n, k];
    }
}
