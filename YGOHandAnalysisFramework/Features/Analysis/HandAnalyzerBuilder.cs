using YGOHandAnalysisFramework.Data;

namespace YGOHandAnalysisFramework.Features.Analysis;

public static class HandAnalyzerBuildArguments
{
    public static HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(string analyzerName, int handSize, IReadOnlyCollection<TCardGroup> cardGroups)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>(analyzerName, handSize, cardGroups);
    }

    public static HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(string analyzerName, int handSize, CardList<TCardGroup, TCardGroupName> cardList)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>(analyzerName, handSize, cardList);
    }

    public static HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> CreateHandAnalyzerBuildArgs<TCardGroup, TCardGroupName>(this CardList<TCardGroup, TCardGroupName> cardList, string analyzerName, int handSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>(analyzerName, handSize, cardList);
    }

    public static HandAnalyzerBuildArguments<TCardGroup, TCardGroupName> CreateHandAnalyzerBuildArgs<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>(handAnalyzer.AnalyzerName, handAnalyzer.HandSize, CardList.Create(handAnalyzer));
    }
}

public record HandAnalyzerBuildArguments<TCardGroup, TCardGroupName>(string AnalyzerName, int HandSize, IReadOnlyCollection<TCardGroup> CardGroups)
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>;
