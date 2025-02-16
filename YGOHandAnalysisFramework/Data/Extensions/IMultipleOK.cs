namespace YGOHandAnalysisFramework.Data.Extensions;

public interface IMultipleOK<TCardGroupName> : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    bool IsMultipleOK { get; }
}
