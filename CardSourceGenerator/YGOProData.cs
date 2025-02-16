using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CardSourceGenerator
{
    internal sealed class YGOProData
    {
        public IReadOnlyList<INamedYGOCard> Data { get; } = Array.Empty<INamedYGOCard>();

        public YGOProData(JsonElement element)
        {
            var list = new List<INamedYGOCard>();
            Data = list;

            if (!element.TryGetProperty("data", out var el))
            {
                throw new Exception("data not found");
            }

            foreach (var obj in el.EnumerateArray())
            {
                list.Add(new YGOProCard(obj));
            }
        }

        public static IReadOnlyList<INamedYGOCard> GetCardData(string text)
        {
            var byteSource = Encoding.UTF8.GetBytes(text);
            var reader = new Utf8JsonReader(byteSource);
            if (!JsonDocument.TryParseValue(ref reader, out var jsonDocument))
            {
                return Array.Empty<INamedYGOCard>();
            }

            return new YGOProData(jsonDocument.RootElement).Data;
        }
    }
}
