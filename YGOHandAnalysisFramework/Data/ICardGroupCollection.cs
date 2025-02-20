namespace YGOHandAnalysisFramework.Data;

public interface ICardGroupCollection<TCardGroup, TCardGroupName> : IReadOnlyCollection<TCardGroup>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    IEnumerable<TCardGroupName> CardNames { get; }
}
