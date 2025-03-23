using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Caching;
using YGOHandAnalysisFramework.Features.Configuration;
using YGOHandAnalysisFramework.Projects;

namespace YGOHandAnalysisFramework.Features.Console;

public abstract class ConsoleAnalyzerComponents<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public abstract IEnumerable<IProject<TCardGroup, TCardGroupName>> CreateProjects(IConfiguration<TCardGroupName> configuration);
    public abstract TCardGroupName CreateCardGroupName(string name);
    public abstract TCardGroup CreateMiscCardGroup(int size);
    public abstract TCardGroup CreateCardGroup(ICardGroup<TCardGroupName> cardGroup);
    public abstract HandAnalyzerLoader<TCardGroup, TCardGroupName> CreateCacheLoader(IConfiguration<TCardGroupName> configuration);

    public virtual IEnumerable<TCardGroupName> GetSupportedCards(IConfiguration<TCardGroupName> configuration)
    {
        return [];
    }

    public virtual IProjectHandler<TCardGroup, TCardGroupName> CreateProjectHandler(IConfiguration<TCardGroupName> configuration)
    {
        return new ProjectHandler<TCardGroup, TCardGroupName>();
    }
}
