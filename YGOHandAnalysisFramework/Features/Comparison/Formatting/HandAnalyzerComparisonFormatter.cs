using System.Text;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Features.Analysis;

namespace YGOHandAnalysisFramework.Features.Comparison.Formatting;

public class HandAnalyzerComparisonFormatter<TCardGroup, TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public HandAnalyzerComparisonFormatter(List<HandAnalyzer<TCardGroup, TCardGroupName>> handAnalyzers, List<IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName>> categoryResults)
    {
        HandAnalyzers = handAnalyzers ?? throw new ArgumentNullException(nameof(handAnalyzers));
        CategoryResults = categoryResults ?? throw new ArgumentNullException(nameof(categoryResults));
    }

    private List<HandAnalyzer<TCardGroup, TCardGroupName>> HandAnalyzers { get; }
    private List<IHandAnalyzerComparisonCategoryResults<TCardGroup, TCardGroupName>> CategoryResults { get; }

    public string FormatResults()
    {
        var numberOfRows = CategoryResults.Count + 1;
        var numberOfColumns = HandAnalyzers.Count + 2;

        var table = new string[numberOfRows, numberOfColumns];

        table[0, 0] = "Category";
        table[0, 1] = "Time (ms)";

        {
            int column = 2;
            foreach (var analyzer in HandAnalyzers)
            {
                table[0, column++] = $"{analyzer.AnalyzerName} ({analyzer.HandSize:N0})";
            }
        }

        {
            int row = 1;

            foreach (var category in CategoryResults)
            {
                var column = 0;
                table[row, column++] = category.Name;
                table[row, column++] = category.ExecutionTime.TotalMilliseconds.ToString("N3");

                foreach (var analyzer in HandAnalyzers)
                {
                    table[row, column++] = category.GetResult(analyzer);
                }

                row++;
            }
        }

        var longestColumn = new int[numberOfColumns];

        for (int row = 0; row < numberOfRows; row++)
        {
            for (int column = 0; column < numberOfColumns; column++)
            {
                if (table[row, column].Length > longestColumn[column])
                {
                    longestColumn[column] = table[row, column].Length;
                }
            }
        }

        int extraPadding = int.MaxValue;
        for (int i = 0; i < longestColumn.Length; i++)
        {
            var padding = longestColumn[i] / 2;
            if (padding < extraPadding)
            {
                extraPadding = padding;
            }
        }

        for (int i = 0; i < longestColumn.Length; i++)
        {
            longestColumn[i] += extraPadding;
        }

        var stringBuilder = new StringBuilder();

        foreach (var handAnalyzer in HandAnalyzers)
        {
            stringBuilder.AppendLine($"Analyzer: {handAnalyzer.AnalyzerName}. Cards: {handAnalyzer.DeckSize:N0}. Hand Size: {handAnalyzer.HandSize:N0}. Possible Hands: {handAnalyzer.Combinations.Count:N0}.");
        }

        stringBuilder.AppendLine();

        for (int row = 0; row < numberOfRows; row++)
        {
            int startingColumn = 0;
            stringBuilder.Append(table[row, startingColumn].PadRight(longestColumn[startingColumn]));

            for (int column = startingColumn + 1; column < numberOfColumns; column++)
            {
                stringBuilder.Append(table[row, column].PadLeft(longestColumn[column]));
            }

            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }
}
