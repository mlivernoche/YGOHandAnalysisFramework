namespace YGOHandAnalysisFramework.Projects;

public interface IProject
{
    string ProjectName { get; }
    void Run(IHandAnalyzerOutputStream outputStream);
}
