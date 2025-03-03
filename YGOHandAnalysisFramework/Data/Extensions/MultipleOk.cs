using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Data.Extensions;

public static class MultipleOk
{
    public static int CountEffectiveCopies<TCardGroupName>(this HandCombination<TCardGroupName> hand, ICardGroup<TCardGroupName> card)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if(card is IMultipleOK<TCardGroupName> multipleOK)
        {
            return hand.CountEffectiveCopies(multipleOK);
        }

        return hand.CountCopiesOfCardInHand(card.Name);
    }

    public static int CountEffectiveCopies<TCardGroup, TCardGroupName>(this HandCombination<TCardGroupName> hand, TCardGroup card)
        where TCardGroup : IMultipleOK<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return card.IsMultipleOK ? hand.CountCopiesOfCardInHand(card.Name) : 1;
    }
}
