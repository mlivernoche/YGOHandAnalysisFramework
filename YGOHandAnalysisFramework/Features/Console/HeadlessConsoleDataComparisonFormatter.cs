using System.Text.Json;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Console.Json;

namespace YGOHandAnalysisFramework.Features.Console;

public sealed class HeadlessConsoleDataComparisonFormatter : IDataComparisonFormatter
{
    private IDataComparisonFormatter Formatter { get; }

    public HeadlessConsoleDataComparisonFormatter(IDataComparisonFormatter formatter)
    {
        Formatter = formatter;
    }

    public HeadlessConsoleDataComparisonFormatter(IEnumerable<IDataComparisonFormatterEntry> entries, IEnumerable<IDataComparisonCategoryResults> categoryResults)
    {
        Formatter = new ConsoleDataComparisonFormatter(entries, categoryResults);
    }

    public string FormatResults()
    {
        var results = Formatter.FormatResults();
        var dto = new MessageDTO()
        {
            Message = results,
        };
        return JsonSerializer.Serialize(dto);
    }
}
