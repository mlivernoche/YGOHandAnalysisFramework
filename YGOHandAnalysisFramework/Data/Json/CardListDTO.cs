namespace YGOHandAnalysisFramework.Data.Json;

public sealed class CardListDTO
{
    public CardGroupDTO[] Cards { get; set; } = [];

    public CardListDTO() { }

    public CardListDTO(IEnumerable<CardGroupDTO> cards)
    {
        Cards = cards.ToArray();
    }

    public static CardListDTO Create<TCardGroupName>(IEnumerable<ICardGroup<TCardGroupName>> cardList, Func<TCardGroupName, string> nameConverter)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cards = new List<CardGroupDTO>();

        foreach (var card in cardList)
        {
            cards.Add(CardGroupDTO.Create(card, nameConverter));
        }

        return new CardListDTO(cards);
    }

    public static CardListDTO Create<TCardGroup, TCardGroupName>(CardList<TCardGroup, TCardGroupName> cardList, Func<TCardGroupName, string> nameConverter)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cards = new List<CardGroupDTO>();

        foreach (var card in cardList)
        {
            cards.Add(CardGroupDTO.Create(card, nameConverter));
        }

        return new CardListDTO(cards);
    }

    public static CardList<ICardGroup<TCardGroupName>, TCardGroupName> Create<TCardGroupName>(CardListDTO cardList, Func<string, TCardGroupName> nameConverter)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cards = new List<ICardGroup<TCardGroupName>>();

        foreach (var card in cardList.Cards)
        {
            cards.Add(CardGroupDTO.Create(card, nameConverter));
        }

        return CardList.Create<ICardGroup<TCardGroupName>, TCardGroupName>(cards);
    }
}
