namespace YGOHandAnalysisFramework.Projects;

public interface IProject
{
    string ProjectName { get; }
    void Run(IHandAnalyzerOutputStream outputStream);
}

public interface IProject<TCardGroupName> : IProject
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    IEnumerable<TCardGroupName> SupportedCards { get; }
}
