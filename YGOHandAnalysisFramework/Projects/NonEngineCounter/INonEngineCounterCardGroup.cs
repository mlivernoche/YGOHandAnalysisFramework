using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Extensions;

namespace YGOHandAnalysisFramework.Projects.NonEngineCounter;

public interface INonEngineCounterCardGroup<TCardGroupName> : ICardGroup<TCardGroupName>, IMultipleOK<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    bool IsNonEngine { get; }
}
