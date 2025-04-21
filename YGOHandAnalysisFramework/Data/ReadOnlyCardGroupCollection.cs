using System.Collections;

namespace YGOHandAnalysisFramework.Data;

public sealed class ReadOnlyCardGroupCollection<TCardGroup, TCardGroupName> : ICardGroupCollection<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup> CardGroups { get; }

    public IEnumerable<TCardGroupName> CardNames => CardGroups.Keys;

    public int Count => CardGroups.Count;

    public ReadOnlyCardGroupCollection()
    {
        CardGroups = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name);
    }

    public ReadOnlyCardGroupCollection(IEnumerable<TCardGroup> cardGroups)
    {
        CardGroups = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name, cardGroups);
    }

    public IEnumerator<TCardGroup> GetEnumerator()
    {
        IEnumerable<TCardGroup> enumerator = CardGroups.Values;
        return enumerator.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerator = CardGroups.Values;
        return enumerator.GetEnumerator();
    }
}
