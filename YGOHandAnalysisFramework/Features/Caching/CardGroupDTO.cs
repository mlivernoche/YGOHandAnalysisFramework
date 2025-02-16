namespace YGOHandAnalysisFramework.Features.Caching
{
    internal sealed class CardGroupDTO
    {
        public string CardName { get; set; } = string.Empty;
        public int Size { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }
    }
}
