namespace YGOHandAnalysisFramework.Data.Formatting;

public interface IFormat<T>
{
    string FormatData(T value);
}
