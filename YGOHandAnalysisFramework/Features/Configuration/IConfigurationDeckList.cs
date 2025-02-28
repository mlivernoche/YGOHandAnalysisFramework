using YGOHandAnalysisFramework.Data;

namespace YGOHandAnalysisFramework.Features.Configuration;

public interface IConfigurationDeckList<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    string Name { get; }
    ICardGroupCollection<CardGroup<TCardGroupName>, TCardGroupName> Cards { get; }
}

public sealed record ConfigurationDeckList<TCardGroupName>(string Name, ICardGroupCollection<CardGroup<TCardGroupName>, TCardGroupName> Cards) : IConfigurationDeckList<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
}
