using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Formatting;

namespace YGOHandAnalysisFramework.Features.Comparison;

public delegate HandAnalyzer<TCardGroup, TCardGroupName> HandAnalyzerOptimizer<TCardGroup, TCardGroupName>(HandAnalyzer<TCardGroup, TCardGroupName> handAnalyzer)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>;

public static partial class HandAnalyzerComparison
{
    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Create<TCardGroup, TCardGroupName>(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> values)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        return new HandAnalyzerComparison<TCardGroup, TCardGroupName>(values);
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> Run<TCardGroup, TCardGroupName>(IHandAnalyzerOutputStream outputStream, HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        comparison.Run(outputStream);
        return comparison;
    }

    public static HandAnalyzerComparison<TCardGroup, TCardGroupName> RunInParallel<TCardGroup, TCardGroupName>(IHandAnalyzerOutputStream outputStream, HandAnalyzerComparison<TCardGroup, TCardGroupName> comparison)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        comparison.RunInParallel(outputStream);
        return comparison;
    }

    public static IDataComparison<TComparison> Create<TComparison>(IEnumerable<TComparison> values)
        where TComparison : IDataComparisonFormatterEntry
    {
        return new DataComparison<TComparison>(values, []);
    }
}

public class HandAnalyzerComparison<TCardGroup, TCardGroupName> : IEnumerable<IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    private List<HandAnalyzer<TCardGroup, TCardGroupName>> Analyzers { get; } = [];
    private List<IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>> Categories { get; } = [];

    public HandAnalyzerComparison(IEnumerable<HandAnalyzer<TCardGroup, TCardGroupName>> analyzers)
    {
        Analyzers.AddRange(analyzers);
    }

    public void Add(IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName> category)
    {
        Categories.Add(category);
    }

    public void Run(IHandAnalyzerOutputStream outputStream)
    {
        var results = new List<IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName>>();

        foreach (var category in Categories)
        {
            results.Add(category.GetResults(Analyzers));
        }

        var formatter = new HandAnalyzerComparisonFormatter<TCardGroup, TCardGroupName>(Analyzers, results);
        outputStream.Write(formatter.FormatResults());
    }

    public void RunInParallel(IHandAnalyzerOutputStream outputStream)
    {
        var list = new List<(int, IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>)>(Categories.Count);

        {
            int sortId = 0;
            foreach (var category in Categories)
            {
                list.Add((sortId++, category));
            }
        }

        var output = new ConcurrentBag<(int SortId, IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName> Result)>();

        Parallel.ForEach(list, tuple =>
        {
            var (sortId, category) = tuple;
            output.Add((sortId, category.GetResults(Analyzers)));
        });

        var results = output.OrderBy(static x => x.SortId).Select(static x => x.Result).ToList();
        var formatter = new HandAnalyzerComparisonFormatter<TCardGroup, TCardGroupName>(Analyzers, results);
        outputStream.Write(formatter.FormatResults());
    }

    public IEnumerator<IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>> GetEnumerator()
    {
        IEnumerable<IHandAnalyzerComparisonCategory<TCardGroup, TCardGroupName>> enumerable = Categories;
        return enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        IEnumerable enumerable = Categories;
        return enumerable.GetEnumerator();
    }
}
