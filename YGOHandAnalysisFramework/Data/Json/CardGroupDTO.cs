namespace YGOHandAnalysisFramework.Data.Json;

public class CardGroupDTO
{
    public string Name { get; set; } = string.Empty;
    public int Size { get; set; }

    public static CardGroupDTO Create<TCardGroupName>(ICardGroup<TCardGroupName> cardGroup, Func<TCardGroupName, string> nameConverter)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardGroupDTO()
        {
            Name = nameConverter(cardGroup.Name),
            Size = cardGroup.Size,
        };
    }

    public static CardGroup<TCardGroupName> Create<TCardGroupName>(CardGroupDTO cardGroup, Func<string, TCardGroupName> nameConverter)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new CardGroup<TCardGroupName>()
        {
            Name = nameConverter(cardGroup.Name),
            Size = cardGroup.Size,
        };
    }
}
