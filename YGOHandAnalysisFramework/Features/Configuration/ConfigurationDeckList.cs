using YGOHandAnalysisFramework.Data;

namespace YGOHandAnalysisFramework.Features.Configuration;

public sealed record ConfigurationDeckList<TCardGroupName>(string Name, ICardGroupCollection<CardGroup<TCardGroupName>, TCardGroupName> Cards) : IConfigurationDeckList<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
}
