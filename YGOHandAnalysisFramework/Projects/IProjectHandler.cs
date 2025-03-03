using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Configuration;

namespace YGOHandAnalysisFramework.Projects;

public interface IProjectHandler<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    void RunProjects(IEnumerable<IProject<TCardGroup, TCardGroupName>> projectsToRun, ICalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>> calculators, IConfiguration<TCardGroupName> configuration);
}
