namespace YGOHandAnalysisFramework.Data.Extensions.Linq;

public static class DictionaryExtensions
{
    public static IEnumerable<KeyValuePair<TKey, TValue>> OrderBy<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, IEnumerable<TKey> order)
    {
        foreach (var key in order)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                yield return new(key, value);
            }
        }
    }
}
