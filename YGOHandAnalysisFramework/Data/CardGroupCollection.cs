using System.Collections;
using System.Diagnostics.Contracts;

namespace YGOHandAnalysisFramework.Data;

public static class CardGroupCollection
{
    [Pure]
    public static CardGroupCollection<TCardGroup, TCardGroupName> Add<TCardGroup, TCardGroupName>(this CardGroupCollection<TCardGroup, TCardGroupName> collection, TCardGroup card)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var list = new List<TCardGroup>(collection)
        {
            card
        };
        return new CardGroupCollection<TCardGroup, TCardGroupName>(list);
    }
}

public class CardGroupCollection<TCardGroup, TCardGroupName> : ICardGroupCollection<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup> CardGroups { get; }

    public IEnumerable<TCardGroupName> CardNames => CardGroups.Keys;

    public int Count => CardGroups.Count;

    public CardGroupCollection()
    {
        CardGroups = new DictionaryWithGeneratedKeys<TCardGroupName, TCardGroup>(static group => group.Name);
    }

    public CardGroupCollection(IEnumerable<TCardGroup> cardGroups)
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
