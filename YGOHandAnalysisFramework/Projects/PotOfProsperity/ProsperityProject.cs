using System.Collections.Immutable;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.Configuration;

namespace YGOHandAnalysisFramework.Projects.PotOfProsperity;

public class ProsperityProject<TCardGroup, TCardGroupName> : IProject<TCardGroup, TCardGroupName>
    where TCardGroup : IProsperityTargetCardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private record Context(TCardGroupName ProsperityName, TCardGroupName MiscName, int BanishNumber);

    private TCardGroupName ProsperityName { get; }
    private TCardGroupName MiscName { get; }

    public string ProjectName => nameof(ProsperityProject<TCardGroup, TCardGroupName>);

    public ProsperityProject(TCardGroupName prosperityName, TCardGroupName miscName)
    {
        ProsperityName = prosperityName;
        MiscName = miscName;
    }

    public void Run(ICalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>> calculators, IConfiguration<TCardGroupName> configuration)
    {
        var probabilityFormatter = new PercentFormat<double>();
        var numericalFormatter = new CardinalFormat<double>();

        void CreateComparison(int banishNumber)
        {
            var globalContext = new Context(ProsperityName, MiscName, banishNumber);
            var comparison = DataComparison
                .Create(calculators)
                .AddCategory($"P(Banish={banishNumber:N0})", probabilityFormatter, globalContext, static (analyzer, context) => CalculateProbability(analyzer, context.ProsperityName, context.MiscName, context.BanishNumber))
                .AddCategory($"Banish={banishNumber:N0} Hit %", probabilityFormatter, globalContext, static (analyzer, context) => CalculateHitPercentage(analyzer, context.ProsperityName, context.MiscName, context.BanishNumber))
                .AddCategory($"EV(Banish={banishNumber:N0})", numericalFormatter, globalContext, static (analyzer, context) => CalculateExpectedValue(analyzer, context.ProsperityName, context.MiscName, context.BanishNumber));

            for(int i = 0; i <= banishNumber; i++)
            {
                var context = new Context(ProsperityName, MiscName, banishNumber);
                comparison = comparison.AddCategory($"P(Find={i:N0})", probabilityFormatter, context, CalculateCertainAmountProb(i));
            }

            comparison
                .RunInParallel(configuration.FormatterFactory)
                .FormatResults()
                .Write(configuration.OutputStream);
        }

        DataComparison
            .Create(calculators)
            .AddCategory("Drawn Prosperity", probabilityFormatter, ProsperityName, static (analyzer, cardName) => analyzer.CalculateProbability(hand => hand.HasThisCard(cardName)))
            .Run(configuration.FormatterFactory)
            .FormatResults()
            .Write(configuration.OutputStream);

        CreateComparison(3);
        CreateComparison(6);
    }

    private static double CalculateProbability(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName prosperityName, TCardGroupName miscName, int prosperityBanishNumber)
    {
        if (!handAnalyzer.CardGroups.ContainsKey(prosperityName))
        {
            return 0.0;
        }

        var totalProb = 0.0;
        var prosperityTargets = handAnalyzer
            .CardGroups
            .Values
            .Where(static group => group.IsProsperityTarget)
            .Select(static group => group.Name)
            .ToImmutableHashSet();
        var optimizedAnalyzer = HandAnalyzer
            .ConvertToCardGroup(handAnalyzer)
            .Optimize(prosperityTargets.Add(prosperityName), miscName)
            .CreateHandAnalyzerBuildArgs(handAnalyzer.AnalyzerName, handAnalyzer.HandSize)
            .CreateHandAnalyzer();

        foreach (var hand in optimizedAnalyzer.Combinations)
        {
            if(!hand.HasThisCard(prosperityName))
            {
                continue;
            }

            var targets = hand
                .GetCardsInHand()
                .ToImmutableHashSet();
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

    private static double CalculateHitPercentage(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName prosperityName, TCardGroupName miscName, int prosperityBanishNumber)
    {
        var probOfHitting = CalculateProbability(handAnalyzer, prosperityName, miscName, prosperityBanishNumber);
        var probOfDrawingProsperity = handAnalyzer.CalculateProbability(hand => hand.HasThisCard(prosperityName));

        if(probOfDrawingProsperity > 0)
        {
            return probOfHitting / probOfDrawingProsperity;
        }

        return 0.0;
    }

    private static double CalculateExpectedValue(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName prosperityName, TCardGroupName miscName, int prosperityBanishNumber)
    {
        if (!handAnalyzer.CardGroups.ContainsKey(prosperityName))
        {
            return 0.0;
        }

        var expectedValue = 0.0;

        var prosperityTargets = handAnalyzer
            .CardGroups
            .Values
            .Where(static group => group.IsProsperityTarget)
            .Select(static group => group.Name)
            .ToImmutableHashSet();
        var optimizedAnalyzer = HandAnalyzer
            .ConvertToCardGroup(handAnalyzer)
            .Optimize(prosperityTargets.Add(prosperityName), miscName)
            .CreateHandAnalyzerBuildArgs(handAnalyzer.AnalyzerName, handAnalyzer.HandSize)
            .CreateHandAnalyzer();

        foreach (var hand in optimizedAnalyzer.Combinations)
        {
            if (!hand.HasThisCard(prosperityName))
            {
                continue;
            }

            var targets = hand
                .GetCardsInHand(optimizedAnalyzer)
                .Select(static group => group.Name)
                .ToImmutableHashSet();
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

    private static Func<HandAnalyzer<TCardGroup, TCardGroupName>, Context, double> CalculateCertainAmountProb(int numberOfTargetsFound)
    {
        return (analyzer, context) => CalculateCertainAmountProb(analyzer, context.ProsperityName, context.MiscName, context.BanishNumber, numberOfTargetsFound);
    }

    private static double CalculateCertainAmountProb(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, TCardGroupName prosperityName, TCardGroupName miscName, int prosperityBanishNumber, int numberOfTargetsFound)
    {
        if (!handAnalyzer.CardGroups.ContainsKey(prosperityName))
        {
            return 0.0;
        }

        var totalProb = 0.0;

        var prosperityTargets = handAnalyzer
            .CardGroups
            .Values
            .Where(static group => group.IsProsperityTarget)
            .Select(static group => group.Name)
            .ToImmutableHashSet();
        var optimizedAnalyzer = HandAnalyzer
            .ConvertToCardGroup(handAnalyzer)
            .Optimize(prosperityTargets.Add(prosperityName), miscName)
            .CreateHandAnalyzerBuildArgs(handAnalyzer.AnalyzerName, handAnalyzer.HandSize)
            .CreateHandAnalyzer();

        foreach (var hand in optimizedAnalyzer.Combinations)
        {
            if (!hand.HasThisCard(prosperityName))
            {
                continue;
            }

            var targets = hand
                .GetCardsInHand(optimizedAnalyzer)
                .Select(static group => group.Name)
                .ToImmutableHashSet();
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
