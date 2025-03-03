namespace YGOHandAnalysisFramework.Data.Extensions;

public interface IMultipleOK<TCardGroupName> : INamedCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    bool IsMultipleOK { get; }
}
