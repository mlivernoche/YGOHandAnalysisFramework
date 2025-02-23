using System.Collections.Immutable;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Projects.PotOfProsperity;

public class ProsperityProject<TCardGroup, TCardGroupName> : IProject
    where TCardGroup : IProsperityTargetCardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private record Context(TCardGroupName ProsperityName, TCardGroupName MiscName, int BanishNumber);

    private IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> HandAnalyzers { get; }
    private TCardGroupName ProsperityName { get; }
    private TCardGroupName MiscName { get; }
    private IDataComparisonFormatterFactory DataComparisonFormatFactory { get; }

    public string ProjectName => nameof(ProsperityProject<TCardGroup, TCardGroupName>);

    public ProsperityProject(
        IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers,
        TCardGroupName prosperityName,
        TCardGroupName miscName,
        IDataComparisonFormatterFactory dataComparisonFormatFactory)
    {
        HandAnalyzers = handAnalyzers ?? throw new ArgumentNullException(nameof(handAnalyzers));
        ProsperityName = prosperityName;
        MiscName = miscName;
        DataComparisonFormatFactory = dataComparisonFormatFactory ?? throw new ArgumentNullException(nameof(dataComparisonFormatFactory));
    }

    public void Run(IHandAnalyzerOutputStream outputStream)
    {
        var probabilityFormatter = new PercentFormat<double>();
        var numericalFormatter = new CardinalFormat<double>();

        void CreateComparison(int banishNumber)
        {
            var comparison = DataComparison
                .Create(HandAnalyzers)
                .AddCategory($"P(Banish={banishNumber:N0})", probabilityFormatter, new Context(ProsperityName, MiscName, banishNumber), CalculateProbability)
                .AddCategory($"Banish={banishNumber:N0} Hit %", probabilityFormatter, new Context(ProsperityName, MiscName, banishNumber), CalculateHitPercentage)
                .AddCategory($"EV(Banish={banishNumber:N0})", numericalFormatter, new Context(ProsperityName, MiscName, banishNumber), CalculateExpectedValue);

            for(int i = 0; i <= banishNumber; i++)
            {
                var context = new Context(ProsperityName, MiscName, banishNumber);
                comparison = comparison.AddCategory($"P(Find={i:N0})", probabilityFormatter, (context, i), static (analyzer, context) => CalculateCertainAmountProb(analyzer, context.context, context.i));
            }

            var results = comparison.RunInParallel(DataComparisonFormatFactory);
            outputStream.Write(results.FormatResults());
        }

        var comparison = DataComparison
            .Create(HandAnalyzers)
            .AddCategory("Drawn Prosperity", probabilityFormatter, new Context(ProsperityName, MiscName, 3), static (handAnalyzer, context) => handAnalyzer.CalculateProbability(context, static (context, hand) => hand.HasThisCard(context.ProsperityName)))
            .Run(DataComparisonFormatFactory);
        outputStream.Write(comparison.FormatResults());

        CreateComparison(3);
        CreateComparison(6);
    }

    private static double CalculateProbability(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context context)
    {
        return CalculateProbability(handAnalyzer, context.ProsperityName, context.MiscName, context.BanishNumber);
    }

    private static double CalculateProbability(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName prosperityName, TCardGroupName miscName, int prosperityBanishNumber)
    {
        if (!handAnalyzer.CardGroups.ContainsKey(prosperityName))
        {
            return 0.0;
        }

        var totalProb = 0.0;

        var prosperityTargets = handAnalyzer.CardGroups.Values.Where(static group => group.IsProsperityTarget).Select(static group => group.Name).ToImmutableHashSet();
        var optimizedAnalyzerCardList = handAnalyzer.CreateSimplifiedCardList(prosperityName, miscName, prosperityTargets);
        var optimizedAnalyzerArgs = HandAnalyzerBuildArguments.Create("Test Analyzer Simplified", handAnalyzer.HandSize, optimizedAnalyzerCardList);
        var optimizedAnalyzer = HandAnalyzer.Create(optimizedAnalyzerArgs);

        foreach (var hand in optimizedAnalyzer.Combinations)
        {
            if(!hand.HasThisCard(prosperityName))
            {
                continue;
            }

            var targets = hand.GetCardsInHand(optimizedAnalyzer).Select(static group => group.Name).ToImmutableHashSet();
            targets = prosperityTargets.Except(targets);

            if(targets.Count == 0)
            {
                continue;
            }

            var prospAnalyzer = optimizedAnalyzer.Excavate(hand, prosperityBanishNumber);
            var probOfProspTargets = 0.0;

            foreach (var prosperityExcavation in prospAnalyzer.Combinations)
            {
                if (prosperityExcavation.HasAnyOfTheseCards(targets))
                {
                    probOfProspTargets += prospAnalyzer.CalculateProbability(prosperityExcavation);
                }
            }

            if(probOfProspTargets > 0)
            {
                totalProb += optimizedAnalyzer.CalculateProbability(hand) * probOfProspTargets;
            }
        }

        return totalProb;
    }

    private static double CalculateHitPercentage(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context context)
    {
        return CalculateHitPercentage(handAnalyzer, context.ProsperityName, context.MiscName, context.BanishNumber);
    }

    private static double CalculateHitPercentage(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName prosperityName, TCardGroupName miscName, int prosperityBanishNumber)
    {
        var probOfHitting = CalculateProbability(handAnalyzer, prosperityName, miscName, prosperityBanishNumber);
        var probOfDrawingProsperity = handAnalyzer.CalculateProbability(prosperityName, static (name, hand) => hand.HasThisCard(name));

        if(probOfDrawingProsperity > 0)
        {
            return probOfHitting / probOfDrawingProsperity;
        }

        return 0.0;
    }

    private static double CalculateExpectedValue(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context context)
    {
        return CalculateExpectedValue(handAnalyzer, context.ProsperityName, context.MiscName, context.BanishNumber);
    }

    private static double CalculateExpectedValue(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName prosperityName, TCardGroupName miscName, int prosperityBanishNumber)
    {
        if (!handAnalyzer.CardGroups.ContainsKey(prosperityName))
        {
            return 0.0;
        }

        var expectedValue = 0.0;

        var prosperityTargets = handAnalyzer.CardGroups.Values.Where(static group => group.IsProsperityTarget).Select(static group => group.Name).ToImmutableHashSet();
        var optimizedAnalyzerCardList = handAnalyzer.CreateSimplifiedCardList(prosperityName, miscName, prosperityTargets);
        var optimizedAnalyzerArgs = HandAnalyzerBuildArguments.Create("Test Analyzer Simplified", handAnalyzer.HandSize, optimizedAnalyzerCardList);
        var optimizedAnalyzer = HandAnalyzer.Create(optimizedAnalyzerArgs);

        foreach (var hand in optimizedAnalyzer.Combinations)
        {
            if (!hand.HasThisCard(prosperityName))
            {
                continue;
            }

            var targets = hand.GetCardsInHand(optimizedAnalyzer).Select(static group => group.Name).ToImmutableHashSet();
            targets = prosperityTargets.Except(targets);

            if (targets.Count == 0)
            {
                continue;
            }

            var prospAnalyzer = optimizedAnalyzer.Excavate(hand, prosperityBanishNumber);
            var targetsFound = 0.0;

            foreach (var prosperityExcavation in prospAnalyzer.Combinations)
            {
                var cardsFound = prosperityExcavation.CountCopiesOfCardInHand(targets);

                if (cardsFound > 0)
                {
                    targetsFound += prospAnalyzer.CalculateProbability(prosperityExcavation) * cardsFound;
                }
            }

            if (targetsFound > 0)
            {
                expectedValue += optimizedAnalyzer.CalculateProbability(hand) * targetsFound;
            }
        }

        return expectedValue;
    }

    private static double CalculateCertainAmountProb(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context context, int numberOfTargetsFound)
    {
        return CalculateCertainAmountProb(handAnalyzer, context.ProsperityName, context.MiscName, context.BanishNumber, numberOfTargetsFound);
    }

    private static double CalculateCertainAmountProb(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName prosperityName, TCardGroupName miscName, int prosperityBanishNumber, int numberOfTargetsFound)
    {
        if (!handAnalyzer.CardGroups.ContainsKey(prosperityName))
        {
            return 0.0;
        }

        var totalProb = 0.0;

        var prosperityTargets = handAnalyzer.CardGroups.Values.Where(static group => group.IsProsperityTarget).Select(static group => group.Name).ToImmutableHashSet();
        var optimizedAnalyzerCardList = handAnalyzer.CreateSimplifiedCardList(prosperityName, miscName, prosperityTargets);
        var optimizedAnalyzerArgs = HandAnalyzerBuildArguments.Create("Test Analyzer Simplified", handAnalyzer.HandSize, optimizedAnalyzerCardList);
        var optimizedAnalyzer = HandAnalyzer.Create(optimizedAnalyzerArgs);

        foreach (var hand in optimizedAnalyzer.Combinations)
        {
            if (!hand.HasThisCard(prosperityName))
            {
                continue;
            }

            var targets = hand.GetCardsInHand(optimizedAnalyzer).Select(static group => group.Name).ToImmutableHashSet();
            targets = prosperityTargets.Except(targets);

            if (targets.Count == 0)
            {
                continue;
            }

            var prospAnalyzer = optimizedAnalyzer.Excavate(hand, prosperityBanishNumber);
            var probOfFindingCertainNumber = 0.0;

            foreach (var prosperityExcavation in prospAnalyzer.Combinations)
            {
                var cardsFound = prosperityExcavation.CountCopiesOfCardInHand(targets);

                if (cardsFound == numberOfTargetsFound)
                {
                    probOfFindingCertainNumber += prospAnalyzer.CalculateProbability(prosperityExcavation);
                }
            }

            if (probOfFindingCertainNumber > 0)
            {
                totalProb += optimizedAnalyzer.CalculateProbability(hand) * probOfFindingCertainNumber;
            }
        }

        return totalProb;
    }
}
