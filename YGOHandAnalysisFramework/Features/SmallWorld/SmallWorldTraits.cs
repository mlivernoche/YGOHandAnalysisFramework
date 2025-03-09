namespace YGOHandAnalysisFramework.Features.SmallWorld;

public sealed record SmallWorldTraits : ISmallWorldTraits
{
    public int Level { get; private init; }
    public int AttackPoints { get; private init; }
    public int DefensePoints { get; private init; }
    public string MonsterType { get; private init; } = string.Empty;
    public string MonsterAttribute { get; private init; } = string.Empty;

    private SmallWorldTraits() { }

    public static bool IsSmallWorldCard(int? level, int? atkPoints, int? defPoints, string? monsterType, string? monsterAttribute)
    {
        if (level is null)
        {
            return false;
        }

        if (atkPoints is null)
        {
            return false;
        }

        if (defPoints is null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(monsterType) || string.IsNullOrWhiteSpace(monsterType))
        {
            return false;
        }

        if (string.IsNullOrEmpty(monsterAttribute) || string.IsNullOrWhiteSpace(monsterAttribute))
        {
            return false;
        }

        return true;
    }

    public static ISmallWorldTraits? TryCreate(int? level, int? atkPoints, int? defPoints, string? monsterType, string? monsterAttribute)
    {
        if (level is null)
        {
            return null;
        }

        if (atkPoints is null)
        {
            return null;
        }

        if (defPoints is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(monsterType) || string.IsNullOrWhiteSpace(monsterType))
        {
            return null;
        }

        if (string.IsNullOrEmpty(monsterAttribute) || string.IsNullOrWhiteSpace(monsterAttribute))
        {
            return null;
        }

        return new SmallWorldTraits
        {
            Level = level.Value,
            AttackPoints = atkPoints.Value,
            DefensePoints = defPoints.Value,
            MonsterType = monsterType,
            MonsterAttribute = monsterAttribute,
        };
    }
}
