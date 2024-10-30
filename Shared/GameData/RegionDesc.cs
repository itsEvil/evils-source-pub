using System.Xml.Linq;

namespace Shared.GameData;
public enum Region {
    None,
    Spawn,
    TownPortals,
    Storage,
    GuildPortal,
}
public sealed class RegionDesc : ObjectDesc {
    public readonly Region Type;
    public RegionDesc(XElement e, uint id, string name) : base(e, id, name) {
        Type = e.ParseEnum<Region>("@type", Region.None);
    }
}
