using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Formatting;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Combinations;
using YGOHandAnalysisFramework.Features.Comparison;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;

namespace YGOHandAnalysisFramework.Features.Assessment;

internal sealed class HandAssessmentWithAnalyzerComparisonCategory<TCardGroup, TCardGroupName, TReturn, TAssessment> : DataComparisonCategoryBase<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    where TAssessment : IHandAssessment<TCardGroupName>
{
    private AssessmentCache<TCardGroup, TCardGroupName, TAssessment> Cache { get; }
    private Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TAssessment> AssessmentFactory { get; }
    private Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> Function { get; }

    public HandAssessmentWithAnalyzerComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> func,
        AssessmentCache<TCardGroup, TCardGroupName, TAssessment> cache,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandAnalyzer<TCardGroup, TCardGroupName>> optimizer)
        : base(name, formatter, optimizer)
    {
        Cache = cache;
        AssessmentFactory = assessmentFactory;
        Function = func;
    }

    public HandAssessmentWithAnalyzerComparisonCategory(
        string name,
        IFormat<TReturn> formatter,
        Func<HandAnalyzer<TCardGroup, TCardGroupName>, HandCombination<TCardGroupName>, TAssessment> assessmentFactory,
        Func<HandAssessmentAnalyzer<TCardGroup, TCardGroupName, TAssessment>, TReturn> func,
        AssessmentCache<TCardGroup, TCardGroupName, TAssessment> cache)
        : this(name, formatter, assessmentFactory, func, cache, static analyzer => analyzer) { }

    protected override TReturn Run(HandAnalyzer<TCardGroup, TCardGroupName> analyzer)
    {
        var value = Cache.GetAnalyzer(analyzer, AssessmentFactory);
        return Function(value);
    }

    protected override IDataComparisonCategoryResults CollateResults(IReadOnlyDictionary<HandAnalyzer<TCardGroup, TCardGroupName>, TReturn> results, TimeSpan executionTime)
    {
        return DataComparisonCategoryResults.Create(Name, Format, results, executionTime);
    }
}
