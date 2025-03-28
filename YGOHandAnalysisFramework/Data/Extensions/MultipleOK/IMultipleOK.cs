namespace YGOHandAnalysisFramework.Data.Extensions.MultipleOK;

public interface IMultipleOK<TCardGroupName> : INamedCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    bool IsMultipleOK { get; }
}
