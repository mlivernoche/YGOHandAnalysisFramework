using CommandLine;
using CommunityToolkit.Diagnostics;
using System.Text;
using System.Text.Json;
using YGOHandAnalysisFramework.Data;
using YGOHandAnalysisFramework.Data.Archive;
using YGOHandAnalysisFramework.Data.Json;
using YGOHandAnalysisFramework.Features.Comparison.Formatting;
using YGOHandAnalysisFramework.Features.Configuration;

namespace YGOHandAnalysisFramework.Features.Console;

public static class ConsoleConfiguration
{
    public static async Task<int> Execute<TCardGroup, TCardGroupName>(string[] args, IReadOnlySet<TCardGroupName> allCardNames, Func<TCardGroupName, string> printName, ConsoleAnalyzerComponents<TCardGroup, TCardGroupName> componentsLoader)
        where TCardGroup : ICardGroup<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
#if DEBUG
        System.Console.WriteLine("Running in DEBUG mode.");
#endif

#if RELEASE
        System.Console.WriteLine("Running in RELEASE mode.");
#endif

        System.Console.WriteLine();
        System.Console.WriteLine($"Parsing console arguments: {string.Join(' ', args)}");

        if (!TryGetOptions(args).GetResult(out var consoleOptions, out var consoleErrors))
        {
            System.Console.Error.WriteLine(CompileConsoleErrors(args, consoleErrors));
            return -1;
        }

        System.Console.WriteLine("Completed parsing console arguments.");
        System.Console.WriteLine();
        System.Console.WriteLine("Opening stdin.");

        if (!TryOpenCardInputStream(consoleOptions).GetResult(out var inputStream, out var inputStreamError))
        {
            var exception = inputStreamError;
            while (exception != null)
            {
                System.Console.Error.WriteLine(exception.Message);
                System.Console.Error.WriteLine(exception.StackTrace);
                exception = exception.InnerException;
            }
            return -2;
        }

        System.Console.WriteLine("stdin opened.");
        System.Console.WriteLine();

        try
        {
            using (inputStream)
            {
                System.Console.WriteLine("Building configuration.");
                var config = await GetConfigurationAsync(consoleOptions, inputStream, componentsLoader.CreateCardGroupName);
                System.Console.WriteLine("Completed building configuration.");
                System.Console.WriteLine();

                if (!config.AreAllCardNamesRecognized(allCardNames, out var cardsNotFound))
                {
                    config.OutputStream.Write("The following cards are not recognized and may impact the results of the models:");

                    foreach (var cardName in cardsNotFound)
                    {
                        config.OutputStream.Write(printName(cardName));
                    }

                    System.Console.WriteLine();
                }

                System.Console.WriteLine("Building analyzers.");
                var analyzersCollection = config.CreateAnalyzers(componentsLoader.CreateMiscCardGroup, componentsLoader.CreateCardGroup, componentsLoader.GetSupportedCards(config).ToHashSet());
                System.Console.WriteLine("Completed building analyzers.");
                System.Console.WriteLine();

                System.Console.WriteLine("Running projects.");
                var handler = componentsLoader.CreateProjectHandler(config);
                handler.RunProjects(componentsLoader.CreateProjects(config), analyzersCollection, config);
                System.Console.WriteLine("Completed running projects.");

                return 0;
            }
        }
        catch (Exception ex)
        {
            var exception = ex;
            while (exception != null)
            {
                System.Console.Error.WriteLine(exception.Message);
                System.Console.Error.WriteLine(exception.StackTrace);
                exception = exception.InnerException;
            }
            return -3;
        }
    }

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

    public static Result<Stream, Exception> TryOpenCardInputStream(ConsoleOptions consoleOptions)
    {
        if(Path.Exists(consoleOptions.CardInputStreamSource))
        {
            return new(new FileStream(consoleOptions.CardInputStreamSource, FileMode.Open, FileAccess.Read));
        }

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
                    cardGroupCollection.Add(CardGroupDTO.Create(card, cardGroupNameFactory));
                }

                yield return new ConfigurationDeckList<TCardGroupName>(deckDTO.Name, cardGroupCollection.ToReadOnly());
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
    public int CardListFillSize { get; }
    public IEnumerable<int> HandSizes { get; }
    public bool CreateWeightedProbabilities { get; }
    public bool UseCache { get; }
    public string CacheLocation { get; }

    public ConsoleConfiguration(ConsoleOptions consoleOptions, IEnumerable<IConfigurationDeckList<TCardGroupName>> deckLists)
    {
        CardListFillSize = consoleOptions.CardListFillSize;
        HandSizes = consoleOptions.HandSizes.ToArray();
        CreateWeightedProbabilities = consoleOptions.CreateWeightedProbabilities;
        UseCache = consoleOptions.UseCache;
        CacheLocation = consoleOptions.CacheLocation;

        DeckLists = deckLists.ToList();
        OutputStream = new HandAnalyzerConsoleOutputStream();
        FormatterFactory = static (analyzers, results) => new HeadlessConsoleDataComparisonFormatter(analyzers, results);
    }
}
