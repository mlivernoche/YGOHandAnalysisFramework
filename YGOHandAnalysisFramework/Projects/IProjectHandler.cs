namespace YGOHandAnalysisFramework.Projects;

public interface IProjectHandler
{
    void RunProjects(IEnumerable<IProject> projectsToRun, IHandAnalyzerOutputStream outputStream);
}

