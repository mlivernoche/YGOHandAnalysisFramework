namespace YGOHandAnalysisFramework.Data.Json;

public sealed class DeckDTO
{
    public string Name { get; set; } = string.Empty;
    public CardListDTO CardList { get; set; } = new CardListDTO();
}
