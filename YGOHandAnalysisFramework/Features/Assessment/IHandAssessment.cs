using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Features.Assessment;

public interface IHandAssessment<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    bool Included { get; }
    HandCombination<TCardGroupName> Hand { get; }
}
