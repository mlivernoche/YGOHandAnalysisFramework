using YGOHandAnalysisFramework.Data.Extensions;

namespace YGOHandAnalysisFramework.Projects.HandComposition;

public interface IHandComposition<TCardGroupName, TCategory> : IMultipleOK<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    where TCategory : notnull, IHandCompositionCategory
{
    TCategory Category { get; }
}
