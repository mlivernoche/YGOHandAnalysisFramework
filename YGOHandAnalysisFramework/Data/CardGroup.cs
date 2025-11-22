namespace YGOHandAnalysisFramework.Data;

public static class CardGroup
{
    public static CardGroup<TCardGroupName> Create<TCardGroupName>(TCardGroupName name, int size, int minimum, int maximum)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardGroup<TCardGroupName>()
        {
            Name = name,
            Size = size,
        };
    }

    public static CardGroup<TCardGroupName> CreateFrom<TCardGroup, TCardGroupName>(TCardGroup cardGroup)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardGroup<TCardGroupName>()
        {
            Name = cardGroup.Name,
            Size = cardGroup.Size,
        };
    }
}

public class CardGroup<TCardGroupName> : ICardGroup<CardGroup<TCardGroupName>, TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public required TCardGroupName Name { get; init; }
    public required int Size { get; init; }

    public CardGroup() { }

    public CardGroup(TCardGroupName name)
    {
        Name = name;
    }

    public CardGroup<TCardGroupName> ChangeSize(int newSize)
    {
        return new CardGroup<TCardGroupName>()
        {
            Name = Name,
            Size = newSize,
        };
    }
}
