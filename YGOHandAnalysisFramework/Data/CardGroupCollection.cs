﻿using System.Collections;
using System.Diagnostics.Contracts;

namespace YGOHandAnalysisFramework.Data;

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

    public void Add(TCardGroup cardGroup)
    {
        CardGroups.AddOrUpdate(cardGroup);
    }

    public ReadOnlyCardGroupCollection<TCardGroup, TCardGroupName> ToReadOnly()
    {
        return new ReadOnlyCardGroupCollection<TCardGroup, TCardGroupName>(this);
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
