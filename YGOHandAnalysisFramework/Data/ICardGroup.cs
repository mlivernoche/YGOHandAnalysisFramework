namespace YGOHandAnalysisFramework.Data;

public interface ICardGroup<TCardGroupName> : INamedCard<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    int Size { get; }
    int Minimum { get; }
    int Maximum { get; }
}

public interface ICardGroup<TCardGroup, TCardGroupName> : ICardGroup<TCardGroupName>
    where TCardGroup : ICardGroup<TCardGroupName>
    where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
{
    TCardGroup ChangeSize(int newSize);
}
