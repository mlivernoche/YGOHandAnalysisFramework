using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Extensions.Linq;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.WeightedProbability;

namespace YGOHandAnalysisFramework.Features.Configuration;

public static class Configuration
{
    public static IReadOnlyCollection<ICalculatorWrapper<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>>> CreateAnalyzers<TCardGroupName>(this IConfiguration<TCardGroupName> config, Func<TCardGroupName> miscNameFactory)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var analyzersCollection = new CalculatorWrapperCollection<HandAnalyzer<CardGroup<TCardGroupName>, TCardGroupName>>();

        foreach (var deckList in config.DeckLists)
        {
            var cardList = CardList
                .Create(deckList.Cards)
                .Fill(config.CardListFillSize, size => CardGroup.Create(miscNameFactory(), size, 0, size));

            var buildArgs = config
                .HandSizes
                .Select(handSize => HandAnalyzerBuildArguments.Create($"{deckList.Name}, {handSize:N0}", handSize, cardList));

            var handAnalyzers = HandAnalyzer.CreateInParallel(buildArgs);
            foreach (var analyzer in handAnalyzers.OrderBy(buildArgs))
            {
                analyzersCollection.Add(analyzer);
            }

            if(config.CreateWeightedProbabilities)
            {
                var weightedProbabilities = WeightedProbabilityCollection.CreateWithEqualWeights(deckList.Name, handAnalyzers.OrderBy(buildArgs));
                analyzersCollection.Add(weightedProbabilities);
            }
        }

        return analyzersCollection;
    }
}
