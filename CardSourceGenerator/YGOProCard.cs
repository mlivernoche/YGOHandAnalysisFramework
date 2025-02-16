using System.Text.Json;

namespace CardSourceGenerator
{
    internal sealed class YGOProCard : INamedYGOCard
    {
        public string Name { get; set; } = string.Empty;

        public YGOProCard(JsonElement element)
        {
            Name = element.TryGetProperty("name");
        }
    }
}
