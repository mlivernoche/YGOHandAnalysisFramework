namespace YGOHandAnalysisFramework.Features.Caching
{
    internal sealed class HandElementDTO
    {
        public string HandName { get; set; } = string.Empty;
        public int MinimumSize { get; set; }
        public int MaximumSize { get; set; }
    }
}
