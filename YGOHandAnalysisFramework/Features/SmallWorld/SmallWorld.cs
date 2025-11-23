using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.CardSearch;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Features.SmallWorld;

public static class SmallWorld
{
    public static bool SmallWorldCanFindCard<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName smallWorldName, TCardGroupName search, HandCombination<TCardGroupName> hand)
        where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>, ISmallWorldCard<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if (!hand.HasThisCard(smallWorldName))
        {
            return false;
        }

        var smallWorldAnalyzer = CardList
            .Create(handAnalyzer)
            .RemoveHand(hand)
            .CreateSmallWorldAnalyzer();

        foreach (var (name, _) in hand.GetCardsInHand())
        {
            if (smallWorldAnalyzer.HasBridge(name, search))
            {
                return true;
            }
        }

        return false;
    }

    public static bool SmallWorldCanFindCard<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName smallWorldName, TCardGroupName search, HandCombination<TCardGroupName> hand, CardSearchNodeCollection<TCardGroupName> searchGraph)
        where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>, ISmallWorldCard<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if (!hand.HasThisCard(smallWorldName))
        {
            return false;
        }

        var cardsInDeck = CardList
            .Create(handAnalyzer)
            .RemoveHand(hand);
        var smallWorldAnalyzer = cardsInDeck.CreateSmallWorldAnalyzer();

        foreach (var (name, _) in hand.GetCardsInHand())
        {
            if (smallWorldAnalyzer.HasBridge(name, search))
            {
                return true;
            }

            if (!handAnalyzer.CardGroups.TryGetValue(name, out var group))
            {
                throw new Exception($"Card in hand \"{name}\" not in card list.");
            }

            foreach (var other in searchGraph.GetCardsAccessibleFromName(name))
            {
                var deckWithoutCard = cardsInDeck.RemoveCardName(other);
                var newSmallWorldAnalyzer = deckWithoutCard.CreateSmallWorldAnalyzer();

                if (newSmallWorldAnalyzer.HasBridge(other, search))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
