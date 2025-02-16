//using System.Text;
//using YGOHandAnalysisFramework.Features.Comparison.Formatting;

//namespace YGOHandAnalysisFramework.Features.Comparison;

//public class DataComparisonFormatter<TComparison> : IDataComparisonFormatter<TComparison>
//    where TComparison : IDataComparisonFormatterEntry
//{
//    public DataComparisonFormatter(IEnumerable<TComparison> handAnalyzers, IEnumerable<IDataComparisonCategoryResults<TComparison>> categoryResults)
//    {
//        HandAnalyzers = new(handAnalyzers);
//        CategoryResults = new(categoryResults);
//    }

//    private List<TComparison> HandAnalyzers { get; }
//    private List<IDataComparisonCategoryResults<TComparison>> CategoryResults { get; }

//    public string FormatResults()
//    {
//        var numberOfRows = CategoryResults.Count + 1;
//        var numberOfColumns = HandAnalyzers.Count + 2;

//        var table = new string[numberOfRows, numberOfColumns];

//        table[0, 0] = "Category";
//        table[0, 1] = "Time (ms)";

//        {
//            int column = 2;
//            foreach (var analyzer in HandAnalyzers)
//            {
//                table[0, column++] = analyzer.GetHeader();
//            }
//        }

//        {
//            int row = 1;

//            foreach (var category in CategoryResults)
//            {
//                var column = 0;
//                table[row, column++] = category.Name;
//                table[row, column++] = category.ExecutionTime.TotalMilliseconds.ToString("N3");

//                foreach (var analyzer in HandAnalyzers)
//                {
//                    table[row, column++] = category.GetResult(analyzer);
//                }

//                row++;
//            }
//        }

//        var longestColumn = new int[numberOfColumns];

//        for (int row = 0; row < numberOfRows; row++)
//        {
//            for (int column = 0; column < numberOfColumns; column++)
//            {
//                if (table[row, column].Length > longestColumn[column])
//                {
//                    longestColumn[column] = table[row, column].Length;
//                }
//            }
//        }

//        int extraPadding = int.MaxValue;
//        for (int i = 0; i < longestColumn.Length; i++)
//        {
//            var padding = longestColumn[i] / 2;
//            if (padding < extraPadding)
//            {
//                extraPadding = padding;
//            }
//        }

//        for (int i = 0; i < longestColumn.Length; i++)
//        {
//            longestColumn[i] += extraPadding;
//        }

//        var stringBuilder = new StringBuilder();

//        foreach (var handAnalyzer in HandAnalyzers)
//        {
//            stringBuilder.AppendLine(handAnalyzer.GetDescription());
//        }

//        stringBuilder.AppendLine();

//        for (int row = 0; row < numberOfRows; row++)
//        {
//            int startingColumn = 0;
//            stringBuilder.Append(table[row, startingColumn].PadRight(longestColumn[startingColumn]));

//            for (int column = startingColumn + 1; column < numberOfColumns; column++)
//            {
//                stringBuilder.Append(table[row, column].PadLeft(longestColumn[column]));
//            }

//            stringBuilder.AppendLine();
//        }

//        return stringBuilder.ToString();
//    }
//}
