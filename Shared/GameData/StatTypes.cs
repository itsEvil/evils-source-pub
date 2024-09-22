namespace Shared.GameData;

public enum StatType : byte {
    Nothing = 0, //should never happen but if it does ignore...
    MaxHealth = 1,
    MaxResource = 2, //mana etc
    Health = 3,
    Resource = 4,

    Inventory = 5, //byte length then array of ItemData
    Stats = 6, //byte length then array of uints
    Condition = 7, //64 bit flag
    Name = 8,
}
