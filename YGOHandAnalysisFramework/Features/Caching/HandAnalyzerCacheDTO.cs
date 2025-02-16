namespace YGOHandAnalysisFramework.Features.Caching
{
    internal sealed class HandAnalyzerCacheDTO
    {
        public CardGroupDTO[] CardGroups { get; set; } = [];
        public HandCombinationDTO[] HandCombinations { get; set; } = [];
    }
}
