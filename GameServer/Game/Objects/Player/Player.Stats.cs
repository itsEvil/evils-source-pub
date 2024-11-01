using Shared;

namespace GameServer.Game.Objects;
//All methods to do with submerged worlds
public enum AttributeType {
    Health,
    Mana,
    Attack,
    Defense,
    Speed,
    Dexterity,
    Vitality,
    Wisdom,
}
public partial class Player {
    private readonly uint[] BaseAttributes = [];
    private readonly uint[] AttributeBoosts = [];
    public uint GetAttribute(int index) => BaseAttributes[index] + AttributeBoosts[index];
    public static int GetAttributeIndex(AttributeType type) {
        return type switch {
            AttributeType.Health    => 0,
            AttributeType.Mana      => 1,
            AttributeType.Attack    => 2,
            AttributeType.Defense   => 3,
            AttributeType.Speed     => 4,
            AttributeType.Dexterity => 5,
            AttributeType.Vitality  => 6,
            AttributeType.Wisdom    => 7,
            _ => 0,
        };
    }
    /// <summary>
    /// Faster then <see cref="GetAttributeIndex(AttributeType)"/> but may cause issues if attributes are not ascending from 0
    /// </summary>
    public static int GetAttributeIndexCast(AttributeType type) {
        return (int)type;
    }
}
