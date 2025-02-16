using YGOHandAnalysisFramework.Data;

namespace YGOHandAnalysisFramework.Features.SmallWorld;

public interface ISmallWorldCard<TCardGroupName> : INamedCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    ISmallWorldTraits? SmallWorldTraits { get; }
    bool CanBeBanished { get; }
}
