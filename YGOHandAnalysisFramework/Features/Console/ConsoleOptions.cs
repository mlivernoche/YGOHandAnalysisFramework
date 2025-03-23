using CommandLine;

namespace YGOHandAnalysisFramework.Features.Console;

public class ConsoleOptions
{
    [Option("fillsize", Default = 40)]
    public int CardListFillSize { get; set; } = 40;

    [Option("handsizes", Default = new[] { 5, 6 })]
    public IEnumerable<int> HandSizes { get; set; } = [5, 6];

    [Option("useweighteds", Default = false)]
    public bool CreateWeightedProbabilities { get; set; } = false;

    [Option("source", Default = "")]
    public string CardInputStreamSource { get; set; } = string.Empty;

    [Option("usecache", Default = false)]
    public bool UseCache { get; set; } = false;

    [Option("cachelocation", Default = "")]
    public string CacheLocation { get; set; } = string.Empty;
}
