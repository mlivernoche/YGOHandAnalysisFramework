namespace YGOHandAnalysisFramework.Features.Caching
{
    internal class HandAnalyzerCacheDTO
    {
        public int HandSize { get; set; }
        public CardGroupDTO[] CardGroups { get; set; } = [];
        public HandCombinationDTO[] HandCombinations { get; set; } = [];
    }
}
