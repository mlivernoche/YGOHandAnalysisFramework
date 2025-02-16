#nullable enable

using System;

namespace CardSourceGenerator
{
    public interface IYGOCard<TCardGroupName>
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        TCardGroupName Name { get; }
        StartingDeckLocation StartingLocation { get; }
        int? Level { get; }
        int? AttackPoints { get; }
        int? DefensePoints { get; }
        string? MonsterType { get; }
        string? MonsterAttribute { get; }
    }
}
