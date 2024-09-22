using System.Xml.Linq;

namespace Shared.GameData;
public sealed class StatsDesc {
    /// <summary>
    /// Index of the stat
    /// </summary>
    public readonly uint Index;
    /// <summary>
    /// Starting value of the stat
    /// </summary>
    public readonly uint Value;
    /// <summary>
    /// Max total value
    /// </summary>
    public readonly uint MaxValue;
    /// <summary>
    /// Cannot be negative
    /// </summary>
    public readonly int MaxIncrease;
    /// <summary>
    /// Cannot be negative
    /// </summary>
    public readonly int MinIncrease;
    public StatsDesc(XElement e) {
        Index = e.ParseUInt("@index");
        Value = e.ParseUInt("@value");
        MaxValue = e.ParseUInt("@maxValue");
        MaxIncrease = e.ParseInt("@max");
        MinIncrease = e.ParseInt("@min");
    }
    public uint GetIncrease() => (uint)Random.Shared.Next(MinIncrease, MaxIncrease);
}
