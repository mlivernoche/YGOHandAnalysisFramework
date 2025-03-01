using System.Collections.Immutable;

namespace YGOHandAnalysisFramework.Projects;

public static class ProjectExtensions
{
    public static IReadOnlySet<TCardGroupName> GetSupportedCards<TCardGroupName>(this IEnumerable<IProject> projects)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return projects
            .OfType<IProject<TCardGroupName>>()
            .SelectMany(static project => project.SupportedCards)
            .ToImmutableHashSet();
    }
}
