namespace YGOHandAnalysisFramework.Data.Operations;

public interface IFilter<T>
{
    IEnumerable<T> GetResults(IEnumerable<T> enumerable);
}
