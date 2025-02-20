using CommunityToolkit.Diagnostics;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using YGOHandAnalysisFramework.Data.Json;

namespace YGOHandAnalysisFramework.Data.Archive;

public static class Archive
{
    public static bool TryOpenArchive(Stream source, [NotNullWhen(true)] out IReader? reader)
    {
        reader = null;

        try
        {
            reader = ReaderFactory.Open(source);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public static async IAsyncEnumerable<DeckDTO> ReadDecksFromArchive(Stream source)
    {
        if(!TryOpenArchive(source, out var reader))
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
                Guard.IsNotNull(deckDTO, nameof(deckDTO));

                yield return deckDTO;
            }
        }
    }

    public static async Task WriteDecksToArchive(Stream destination, IEnumerable<DeckDTO> decks)
    {
        using var writer = WriterFactory.Open(destination, ArchiveType.Tar, new WriterOptions(CompressionType.None));

        foreach(var deckDTO in decks)
        {
            var fileName = $"{deckDTO.Name}.json";

            await using var deckStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(deckStream, deckDTO);

            // reset position so writer can use it.
            deckStream.Position = 0;

            writer.Write(fileName, deckStream);
        }
    }
}
