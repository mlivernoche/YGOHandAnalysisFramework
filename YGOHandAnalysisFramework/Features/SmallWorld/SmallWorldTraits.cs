using CardSourceGenerator;

namespace YGOHandAnalysisFramework.Features.SmallWorld;

public sealed record SmallWorldTraits : ISmallWorldTraits
{
    public int Level { get; private init; }
    public int AttackPoints { get; private init; }
    public int DefensePoints { get; private init; }
    public string MonsterType { get; private init; } = string.Empty;
    public string MonsterAttribute { get; private init; } = string.Empty;

    private SmallWorldTraits() { }

    public static bool IsSmallWorldCard<TCardGroupName>(IYGOCard<TCardGroupName> cardData)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if (cardData.Level is null)
        {
            return false;
        }

        if (cardData.AttackPoints is null)
        {
            return false;
        }

        if (cardData.DefensePoints is null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(cardData.MonsterType) || string.IsNullOrWhiteSpace(cardData.MonsterType))
        {
            return false;
        }

        if (string.IsNullOrEmpty(cardData.MonsterAttribute) || string.IsNullOrWhiteSpace(cardData.MonsterAttribute))
        {
            return false;
        }

        return true;
    }

    public static ISmallWorldTraits? TryCreate<TCardGroupName>(IYGOCard<TCardGroupName> cardData)
        where TCardGroupName : notnull, IEquatable<TCardGroupName>, IComparable<TCardGroupName>
    {
        if (cardData.Level is null)
        {
            return null;
        }

        if (cardData.AttackPoints is null)
        {
            return null;
        }

        if (cardData.DefensePoints is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(cardData.MonsterType) || string.IsNullOrWhiteSpace(cardData.MonsterType))
        {
            return null;
        }

        if (string.IsNullOrEmpty(cardData.MonsterAttribute) || string.IsNullOrWhiteSpace(cardData.MonsterAttribute))
        {
            return null;
        }

        return new SmallWorldTraits
        {
            Level = cardData.Level.Value,
            AttackPoints = cardData.AttackPoints.Value,
            DefensePoints = cardData.DefensePoints.Value,
            MonsterType = cardData.MonsterType,
            MonsterAttribute = cardData.MonsterAttribute,
        };
    }
}
