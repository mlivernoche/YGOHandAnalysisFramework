using CommandLine;
using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Text;
using System.Text.Json;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Archive;
using YGOHandAnalysisFramework.Data.Json;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Configuration;
using YGOHandAnalysisFramework.Features.Console.Json;

namespace YGOHandAnalysisFramework.Features.Console;

public static class ConsoleConfiguration
{
    public static Result<ConsoleOptions, IEnumerable<Error>> TryGetOptions(string[] args)
    {
        Result<ConsoleOptions, IEnumerable<Error>> result = new();
        Parser
            .Default
            .ParseArguments<ConsoleOptions>(args)
            .WithParsed(options =>
            {
                result = new(options);
            })
            .WithNotParsed(errors =>
            {
                result = new(errors);
            });
        return result;
    }

    public static Result<ConsoleOptions, IEnumerable<Error>> TryGetOptions(string args)
    {
        return TryGetOptions(args.SplitArgs());
    }

    public static Result<Stream, Exception> TryOpenStandardInputStream()
    {
        try
        {
            return new(System.Console.OpenStandardInput());
        }
        catch(Exception exception)
        {
            return new(exception);
        }
    }

    public static string CompileConsoleErrors(string[] args, IEnumerable<Error> errors)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendJoin(' ', args);
        stringBuilder.AppendLine();

        int i = 1;
        foreach(var error in errors)
        {
            stringBuilder.AppendLine($"Error #{i++}: {error.Tag.ToString()}");
        }

        return stringBuilder.ToString();
    }

    public static string CreateCommandLineArguments(ConsoleOptions consoleOptions)
    {
        return Parser.Default.FormatCommandLine(consoleOptions);
    }

    public static async Task<IConfiguration<TCardGroupName>> GetConfigurationAsync<TCardGroupName>(ConsoleOptions options, Stream stream, Func<string, TCardGroupName> cardGroupNameFactory)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        var listOfCardLists = new List<IConfigurationDeckList<TCardGroupName>>();

        await foreach (var deckList in LoadCardGroups(stream, cardGroupNameFactory))
        {
            var cardList = deckList.Cards;
            Guard.IsGreaterThanOrEqualTo(cardList.Count, 0);

            if (cardList.Count == 0)
            {
                continue;
            }

            var numberOfCardsInCardList = cardList.Sum(static group => group.Size);
            Guard.IsGreaterThanOrEqualTo(numberOfCardsInCardList, 0);

            if (numberOfCardsInCardList == 0)
            {
                continue;
            }

            listOfCardLists.Add(new ConfigurationDeckList<TCardGroupName>(deckList.Name, cardList));
        }

        return new ConsoleConfiguration<TCardGroupName>(options, listOfCardLists);
    }

    private static async IAsyncEnumerable<IConfigurationDeckList<TCardGroupName>> LoadCardGroups<TCardGroupName>(Stream stream, Func<string, TCardGroupName> cardGroupNameFactory)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if (!Archive.TryOpenArchive(stream, out var reader))
        {
            yield break;
        }

        using (reader)
        {
            while (reader.MoveToNextEntry())
            {
                if (reader.Entry.IsDirectory)
                {
                    continue;
                }

                await using var entryStream = reader.OpenEntryStream();
                var deckDTO = await JsonSerializer.DeserializeAsync<DeckDTO>(entryStream);
                Guard.IsNotNull(deckDTO);
                var cards = deckDTO.CardList?.Cards;
                Guard.IsNotNull(cards);

                var cardGroupCollection = new CardGroupCollection<CardGroup<TCardGroupName>, TCardGroupName>();

                foreach (var card in cards)
                {
                    cardGroupCollection = cardGroupCollection.Add(CardGroupDTO.Create(card, cardGroupNameFactory));
                }

                yield return new ConfigurationDeckList<TCardGroupName>(deckDTO.Name, cardGroupCollection);
            }
        }
    }
}

public sealed class ConsoleConfiguration<TCardGroupName> : IConfiguration<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    public IEnumerable<IConfigurationDeckList<TCardGroupName>> DeckLists { get; }
    public IHandAnalyzerOutputStream OutputStream { get; }
    public CreateDataComparisonFormatter FormatterFactory { get; }
    public int CardListFillSize { get; } = 40;
    public IEnumerable<int> HandSizes { get; } = [5, 6];
    public bool CreateWeightedProbabilities { get; } = true;

    public ConsoleConfiguration(ConsoleOptions consoleOptions, IEnumerable<IConfigurationDeckList<TCardGroupName>> deckLists)
    {
        CardListFillSize = consoleOptions.CardListFillSize;
        HandSizes = consoleOptions.HandSizes.ToArray();
        CreateWeightedProbabilities = consoleOptions.CreateWeightedProbabilities;

        DeckLists = deckLists.ToList();
        OutputStream = new HandAnalyzerConsoleOutputStream();
        FormatterFactory = static (analyzers, results) => new HeadlessConsoleDataComparisonFormatter(analyzers, results);
    }
}
