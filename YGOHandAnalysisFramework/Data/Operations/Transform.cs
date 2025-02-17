using CommunityToolkit.Diagnostics;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;

namespace YGOHandAnalysisFramework.Data.Operations;

public static class Transform
{
    public static IEnumerable<(TCardGroup, HandElement<TCardGroupName>)> GetCardGroups<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> analyzer, HandCombination<TCardGroupName> hand)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var card in hand.GetCardsInHand())
        {
            if (analyzer.CardGroups.TryGetValue(card.HandName, out var cardGroup))
            {
                yield return (cardGroup, card);
            }
        }
    }

    public delegate TCardGroup CreateMiscCardGroup<TCardGroup, TCardGroupName>(int size, int minSize, int maxSize)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>;

    public static HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName> Optimize<TCardGroupName>(this HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName> analyzer, IEnumerable<TCardGroupName> cardsToSave, TCardGroupName miscCardGroupName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return analyzer.Optimize(cardsToSave, (size, min, max) =>
        {
            return new CardGroup<TCardGroupName>()
            {
                Name = miscCardGroupName,
                Size = size,
                Minimum = min,
                Maximum = max,
            };
        });
    }

    public static IEnumerable<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>> OptimizeAll<TCardGroupName>(this IEnumerable<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>> handAnalyzers, IEnumerable<TCardGroupName> cardsToSave, TCardGroupName miscCardGroupName)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach(var analyzer in handAnalyzers)
        {
            yield return analyzer.Optimize(cardsToSave, miscCardGroupName);
        }
    }

    public static HandAnalyzer<TCardGroup, TCardGroupName> Optimize<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> analyzer, IEnumerable<TCardGroupName> cardsToSave, CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardGroups = new HashSet<TCardGroup>();
        var cardsToSaveCopy = cardsToSave is IReadOnlySet<TCardGroupName> names ? names : new HashSet<TCardGroupName>(cardsToSave);

        foreach (var (cardGroupName, cardGroup) in analyzer.CardGroups)
        {
            if (cardsToSaveCopy.Contains(cardGroupName))
            {
                cardGroups.Add(cardGroup);
            }
        }

        var cardSize = cardGroups.Sum(static group => group.Size);
        Guard.IsLessThanOrEqualTo(cardSize, analyzer.DeckSize);
        Guard.IsGreaterThan(cardSize, 0);
        var miscSize = analyzer.DeckSize - cardSize;
        var misc = miscFactory(miscSize, 0, miscSize);
        cardGroups.Add(misc);

        var cardList = CardList.Create<TCardGroup, TCardGroupName>(cardGroups);
        var analyzerArgs = HandAnalyzerBuildArguments.Create(analyzer.AnalyzerName, analyzer.HandSize, cardList);
        return HandAnalyzer.Create(analyzerArgs);
    }

    public static IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> Optimize<TCardGroup, TCardGroupName>(this IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers, IEnumerable<TCardGroupName> cardsToSave, CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var optimizedAnalyzers = new HashSet<HandAnalyzer<TCardGroup, TCardGroupName>>();

        foreach (var analyzer in analyzers)
        {
            optimizedAnalyzers.Add(analyzer.Optimize(cardsToSave, miscFactory));
        }

        return optimizedAnalyzers;
    }

    public static IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> Optimize<TCardGroup, TCardGroupName>(this IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers, Func<HandAnalyzer<TCardGroup, TCardGroupName>, IEnumerable<TCardGroupName>> cardsToSave, CreateMiscCardGroup<TCardGroup, TCardGroupName> miscFactory)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var optimizedAnalyzers = new HashSet<HandAnalyzer<TCardGroup, TCardGroupName>>();

        foreach (var analyzer in analyzers)
        {
            optimizedAnalyzers.Add(analyzer.Optimize(cardsToSave(analyzer), miscFactory));
        }

        return optimizedAnalyzers;
    }

    public static HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName> ConvertToCardGroup<TCardGroup, TCardGroupName>(this HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var cardList = CardList
            .Create(handAnalyzer)
            .Cast(static group => new CardGroup<TCardGroupName>()
            {
                Name = group.Name,
                Size = group.Size,
                Minimum = group.Minimum,
                Maximum = group.Maximum,
            });
        var buildArgs = HandAnalyzerBuildArguments.Create(handAnalyzer.AnalyzerName, handAnalyzer.HandSize, cardList);
        return new HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>(buildArgs, handAnalyzer.Combinations);
    }


    public static IEnumerable<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>> ConvertToCardGroup<TCardGroup, TCardGroupName>(this IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        foreach (var handAnalyzer in handAnalyzers)
        {
            yield return handAnalyzer.ConvertToCardGroup();
        }
    }
}
