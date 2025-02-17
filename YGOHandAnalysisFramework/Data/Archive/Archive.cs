using SharpCompress.Common;
using SharpCompress.Writers;
using System.Text.Json;
using YGOHandAnalysisFramework.Data.Json;

namespace YGOHandAnalysisFramework.Data.Archive;

public static class Archive
{
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
