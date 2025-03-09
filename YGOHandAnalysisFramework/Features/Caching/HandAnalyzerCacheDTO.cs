namespace YGOHandAnalysisFramework.Features.Caching
{
    internal class HandAnalyzerCacheDTO
    {
        public CardGroupDTO[] CardGroups { get; set; } = [];
        public HandCombinationDTO[] HandCombinations { get; set; } = [];
    }
}
