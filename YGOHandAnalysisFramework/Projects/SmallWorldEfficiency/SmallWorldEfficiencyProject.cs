using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Features.Comparison.Calculator;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Configuration;
using YGOHandAnalysisFramework.Features.SmallWorld;

namespace YGOHandAnalysisFramework.Projects.SmallWorldEfficiency;

public sealed class SmallWorldEfficiencyProject<TCardGroup, TCardGroupName> : IProject<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>, ISmallWorldCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private record Context(TCardGroupName SmallWorldName, TCardGroupName MiscName, HashSet<TCardGroupName> SearchTargets);

    private TCardGroupName SmallWorldName { get; }
    private TCardGroupName MiscName { get; }
    private HashSet<TCardGroupName> SearchTargets { get; }

    public string ProjectName => nameof(SmallWorldEfficiencyProject<TCardGroup, TCardGroupName>);

    public IReadOnlySet<TCardGroupName> SupportedCardNames { get; }

    public SmallWorldEfficiencyProject(
        TCardGroupName smallWorldName,
        TCardGroupName miscName,
        IEnumerable<TCardGroupName> targets)
    {
        SmallWorldName = smallWorldName;
        MiscName = miscName;
        SearchTargets = [.. targets];
        SupportedCardNames = new HashSet<TCardGroupName>([.. SearchTargets, SmallWorldName]);
    }

    public void Run(ICalculatorWrapperCollection<HandAnalyzer<TCardGroup, TCardGroupName>> calculators, IConfiguration<TCardGroupName> configuration)
    {
        var context = new Context(SmallWorldName, MiscName, SearchTargets);

        DataComparison
            .Create(calculators)
            .AddCategory("Hand Has No Search Targets", PercentFormat<double>.Default, context, ProbabilityOfNoSearchTargets)
            .AddCategory("Hand Has A Monster To Reveal", PercentFormat<double>.Default, context, ProbabilityOfHavingARevealCard)
            .AddCategory("Hand Has Small World", PercentFormat<double>.Default, context.SmallWorldName, HasSmallWorld)
            .AddCategory("Small World Can Find A Target", PercentFormat<double>.Default, context, CanSmallWorldFindCard)
            .AddCategory("Small World Can Find A Target (Net)", PercentFormat<double>.Default, context, static (analyzer, args) => analyzer.CalculateProbability(args, CanSmallWorldFindCardNet))
            .AddCategory("Small World Efficiency", PercentFormat<double>.Default, context, static (analyzer, args) =>
            {
                var hasCard = HasSmallWorld(analyzer, args.SmallWorldName);
                var canFind = CanSmallWorldFindCard(analyzer, args);

                if (hasCard == 0.0)
                {
                    return 0.0;
                }

                return canFind / hasCard;
            })
            .AddCategory("Small World Efficiency (Net)", PercentFormat<double>.Default, context, static (analyzer, args) =>
            {
                var hasCard = HasSmallWorld(analyzer, args.SmallWorldName);
                var canFind = analyzer.CalculateProbability(args, CanSmallWorldFindCardNet);

                if (hasCard == 0.0)
                {
                    return 0.0;
                }

                return canFind / hasCard;
            })
            .RunInParallel(configuration.FormatterFactory)
            .FormatResults()
            .Write(configuration.OutputStream);
    }

    private static double ProbabilityOfHavingARevealCard(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context args)
    {
        return handAnalyzer.CalculateProbability(args, static (analyzer, hand, args) =>
        {
            if(!hand.HasThisCard(args.SmallWorldName))
            {
                return false;
            }

            foreach(var card in hand.GetCardsInHand(analyzer))
            {
                if(card.CanBeBanished && card.SmallWorldTraits is not null)
                {
                    return true;
                }
            }

            return false;
        });
    }

    private static double ProbabilityOfNoSearchTargets(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context args)
    {
        return handAnalyzer.CalculateProbability(args, static (hand, args) =>
        {
            if (!hand.HasThisCard(args.SmallWorldName))
            {
                return false;
            }

            var numberOfSearchTargets = 0;
            var foundSearchTargets = 0;

            foreach (var searchTarget in args.SearchTargets)
            {
                numberOfSearchTargets++;

                if (hand.HasThisCard(searchTarget))
                {
                    foundSearchTargets++;
                }
            }

            return numberOfSearchTargets == foundSearchTargets;
        });
    }

    private static double HasSmallWorld(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, TCardGroupName name)
    {
        return analyzer.CalculateProbability(name, static (hand, name) => hand.HasThisCard(name));
    }

    private static double CanSmallWorldFindCard(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context args)
    {
        return handAnalyzer.CalculateProbability(args, (analyzer, hand, context) =>
        {
            if (!hand.HasThisCard(context.SmallWorldName))
            {
                return false;
            }

            foreach (var searchTarget in context.SearchTargets)
            {
                if (analyzer.SmallWorldCanFindCard(context.SmallWorldName, searchTarget, hand))
                {
                    return true;
                }
            }

            return false;
        });
    }

    private static bool CanSmallWorldFindCardNet(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, HandCombination<TCardGroupName> hand, Context args)
    {
        if (!hand.HasThisCard(args.SmallWorldName))
        {
            return false;
        }

        foreach (var searchTarget in args.SearchTargets)
        {
            if(hand.HasThisCard(searchTarget))
            {
                continue;
            }

            if (handAnalyzer.SmallWorldCanFindCard(args.SmallWorldName, searchTarget, hand))
            {
                return true;
            }
        }

        return false;
    }
}
