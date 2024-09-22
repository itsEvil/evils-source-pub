using Shared.Redis.Models;
using System.Xml.Linq;

namespace Shared.GameData;
public enum StatValues
{
    Value,
    MaxValue,
    MaxIncrease,
    MinIncrease,
}
public sealed class PlayerDesc : ObjectDesc
{
    /// <summary>
    /// Starting stats
    /// </summary>
    public readonly StatsDesc[] Stats = [];
    /// <summary>
    /// Starting inventory
    /// </summary>
    public readonly ItemData[] Inventory = [];
    /// <summary>
    /// Starting level
    /// </summary>
    public readonly uint Level = 1;
    /// <summary>
    /// Starting exp
    /// </summary>
    public readonly uint Exp = 0;
    /// <summary>
    /// Starting exp goal
    /// </summary>
    public readonly uint ExpGoal = 0;
    /// <summary>
    /// Array of all the stat starting values
    /// </summary>
    public readonly uint[] StatValues = [];
    /// <summary>
    /// Array of all the max stat values
    /// </summary>
    public readonly uint[] StatMaxValues = [];
    public PlayerDesc(XElement e, uint type, string id) : base(e, type, id) {
        Level = e.ParseUInt("Level");
        Exp = e.ParseUInt("Exp");
        ExpGoal = e.ParseUInt("ExpGoal");

        var inv = e.Element("Inventory");
        if(inv != null)
        {
            Inventory = ParseInventory(inv);
        }

        var stats = e.Element("Stats");
        if (stats != null)
            Stats = ParseStats(stats);

        StatMaxValues = new uint[Stats.Length];
        for (int i = 0; i < Stats.Length; i++)
            StatMaxValues[i] = Stats[i].MaxValue;

        StatValues = new uint[Stats.Length];
        for (int i = 0; i < Stats.Length; i++)
            StatValues[i] = Stats[i].Value;
    }
    private StatsDesc[] ParseStats(XElement e) {
        var size = e.ParseUInt("@size");
        if(size == 0)
            return [];

        var stats = new StatsDesc[size];
        foreach(var item in e.Elements("Stat")) {
            var stat = new StatsDesc(item);
            if (stat.Index >= stats.Length)
                throw new Exception("PlayerDesc Stat Index is greater or equal to Stats Length");

            stats[stat.Index] = stat;
        }

        return new StatsDesc[size];
    }
    private ItemData[] ParseInventory(XElement e)
    {
        var size = e.ParseUInt("@size", undefined: 0);
        if (size == 0)
            return [];

        var empty = e.ParseUInt("@empty", undefined: Resources.EmptyItemId);
        var emptyStack = e.ParseUInt("@emptyStack");

        var inventory = new ItemData[size];
        int idx = 0;

        foreach(var item in e.Elements("Item")) {
            var id = item.ParseUInt("@id");
            var stack = item.ParseUInt("@stack");
            if (id == 0 || stack == 0)
                continue;

            inventory[idx++] = new ItemData(0, id, stack);
        }

        for (int i = idx; i < inventory.Length; i++)
            inventory[i] = new ItemData(0, empty, emptyStack);

        return inventory;
    }
}
