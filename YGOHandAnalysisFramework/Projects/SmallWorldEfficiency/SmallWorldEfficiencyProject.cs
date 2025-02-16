using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Data.Operations;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Features.SmallWorld;
using YGOHandAnalysisFramework.Projects.PotOfProsperity;

namespace YGOHandAnalysisFramework.Projects.SmallWorldEfficiency;

public sealed class SmallWorldEfficiencyProject<TCardGroup, TCardGroupName> : IProject
    where TCardGroup : ICardGroup<TCardGroup, TCardGroupName>, ISmallWorldCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private record Context(TCardGroupName SmallWorldName, TCardGroupName MiscName, HashSet<TCardGroupName> SearchTargets);

    private HashSet<HandAnalyzer<TCardGroup, TCardGroupName>> HandAnalyzers { get; }
    private TCardGroupName SmallWorldName { get; }
    private TCardGroupName MiscName { get; }
    private HashSet<TCardGroupName> SearchTargets { get; }
    private CreateDataComparisonFormat DataComparisonFormatFactory { get; }

    public string ProjectName => nameof(SmallWorldEfficiencyProject<TCardGroup, TCardGroupName>);

    public SmallWorldEfficiencyProject(
        IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers,
        TCardGroupName smallWorldName,
        TCardGroupName miscName,
        IEnumerable<TCardGroupName> targets,
        CreateDataComparisonFormat dataComparisonFormatFactory)
    {
        HandAnalyzers = new(handAnalyzers);
        SmallWorldName = smallWorldName;
        MiscName = miscName;
        SearchTargets = new(targets);
        DataComparisonFormatFactory = dataComparisonFormatFactory;
    }

    public void Run(IHandAnalyzerOutputStream outputStream)
    {
        var context = new Context(SmallWorldName, MiscName, SearchTargets);

        var comparison = DataComparison
            .Create(HandAnalyzers)
            .Add("Hand Has No Search Targets", PercentFormat<double>.Default, context, static (analyzer, args) => analyzer.CalculateProbability(args, static (args, hand) =>
            {
                if (!hand.HasThisCard(args.SmallWorldName))
                {
                    return false;
                }

                var numberOfSearchTargets = 0;
                var foundSearchTargets = 0;

                foreach(var searchTarget in args.SearchTargets)
                {
                    numberOfSearchTargets++;

                    if (hand.HasThisCard(searchTarget))
                    {
                        foundSearchTargets++;
                    }
                }

                return numberOfSearchTargets == foundSearchTargets;
            }))
            .Add("Hand Has Small World", PercentFormat<double>.Default, context, static (analyzer, args) => analyzer.CalculateProbability(args, HasSmallWorld))
            .Add("Small World Can Find A Target", PercentFormat<double>.Default, context, (analyzer, args) => analyzer.CalculateProbability(args, CanSmallWorldFindCard))
            .Add("Small World Can Find A Target (Net)", PercentFormat<double>.Default, context, (analyzer, args) => analyzer.CalculateProbability(args, CanSmallWorldFindCardNet))
            .Add("Small World Efficiency", PercentFormat<double>.Default, context, static (analyzer, args) =>
            {
                var hasCard = analyzer.CalculateProbability(args, HasSmallWorld);
                var canFind = analyzer.CalculateProbability(args, CanSmallWorldFindCard);

                if(hasCard == 0.0)
                {
                    return 0.0;
                }

                return canFind / hasCard;
            })
            .Add("Small World Efficiency (Net)", PercentFormat<double>.Default, context, static (analyzer, args) =>
            {
                var hasCard = analyzer.CalculateProbability(args, HasSmallWorld);
                var canFind = analyzer.CalculateProbability(args, CanSmallWorldFindCardNet);

                if (hasCard == 0.0)
                {
                    return 0.0;
                }

                return canFind / hasCard;
            })
            .RunInParallel(DataComparisonFormatFactory)
            .FormatResults();
        outputStream.Write(comparison);
    }

    private static bool HasSmallWorld(Context args, HandCombination<TCardGroupName> hand)
    {
        return hand.HasThisCard(args.SmallWorldName);
    }

    private static bool CanSmallWorldFindCard(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context args, HandCombination<TCardGroupName> hand)
    {
        if (!hand.HasThisCard(args.SmallWorldName))
        {
            return false;
        }

        foreach (var searchTarget in args.SearchTargets)
        {
            if (handAnalyzer.SmallWorldCanFindCard(args.SmallWorldName, searchTarget, hand))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanSmallWorldFindCardNet(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer, Context args, HandCombination<TCardGroupName> hand)
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
