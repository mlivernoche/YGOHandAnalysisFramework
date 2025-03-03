using System.Diagnostics;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Configuration;

namespace YGOHandAnalysisFramework.Projects;

public sealed class ProjectHandler<TCardGroup, TCardGroupName> : IProjectHandler<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public void RunProjects(IEnumerable<IProject<TCardGroup, TCardGroupName>> projectsToRun, ICalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>> calculators, IConfiguration<TCardGroupName> configuration)
    {
        var collection = new List<IProject<TCardGroup, TCardGroupName>>(projectsToRun);

        configuration.OutputStream.Write($"{nameof(ProjectHandler<TCardGroup, TCardGroupName>)}: running {collection.Count:N0} project(s).");

        int i = 1;
        foreach(var project in collection)
        {
            configuration.OutputStream.Write($"{nameof(ProjectHandler<TCardGroup, TCardGroupName>)}: running project #{i:N0} ({project.ProjectName}).");
            var stopwatch = Stopwatch.StartNew();
            project.Run(calculators, configuration);
            stopwatch.Stop();
            configuration.OutputStream.Write($"{nameof(ProjectHandler<TCardGroup, TCardGroupName>)}: finished project #{i:N0} ({project.ProjectName}). {stopwatch.Elapsed.TotalMilliseconds:N3} ms.");
            i++;
        }

        configuration.OutputStream.Write($"{nameof(ProjectHandler<TCardGroup, TCardGroupName>)}: finished running project(s).");
    }
}
