using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using System.Collections.Concurrent;

namespace YGOHandAnalysisFramework.Features.Assessment;

internal sealed class AssessmentCache<TCardGroup, TCardGroupName, TAssessment>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    where TAssessment : IHandAssessment<TCardGroupName>
{
    private ConcurrentDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>> AssessmentAnalyzers { get; } = new();
    private readonly Lock _lock = new();

    public HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> GetAnalyzer(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TAssessment> assessmentFactory)
    {
        lock (_lock)
        {
            if (!AssessmentAnalyzers.TryGetValue(analyzer, out HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>? value))
            {
                value = analyzer.AssessHands(assessmentFactory);
                AssessmentAnalyzers[analyzer] = value;
            }

            return value;
        }
    }

    public HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment> GetAnalyzer(HandAnalyzer<TCardGroup, TCardGroupName> analyzer, Func<HandCombination<TCardGroupName>, TAssessment> assessmentFactory)
    {
        lock (_lock)
        {
            if (!AssessmentAnalyzers.TryGetValue(analyzer, out HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>? value))
            {
                value = analyzer.AssessHands(assessmentFactory);
                AssessmentAnalyzers[analyzer] = value;
            }

            return value;
        }
    }
}
