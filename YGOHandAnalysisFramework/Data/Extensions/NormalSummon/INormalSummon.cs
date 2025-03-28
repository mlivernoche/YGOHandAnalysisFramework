namespace YGOHandAnalysisFramework.Data.Extensions.NormalSummon;

public interface INormalSummon<TCardGroupName> : INamedCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    /// <summary>
    /// This does not describe whether or not a card can be normal summoned. Rather, it is
    /// talking about cards like Lava Golem or Sphere Mode, where normal summon is the card's
    /// only use. Something like Ext Ryzeal would require a specific implementation.
    /// </summary>
    bool RequiresNormalSummon { get; }
}
