namespace YGOHandAnalysisFramework.Projects.HandComposition;

public interface IHandCompositionCategory
{
    string Name { get; }
    int NetCardEconomy { get; }
    IComparer<double> ValueComparer { get; }
}
