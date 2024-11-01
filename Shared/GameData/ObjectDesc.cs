using System.Xml.Linq;

namespace Shared.GameData;
public enum ClassType {
    GameObject,
    Item,
    Player,
    Enemy,
    Projectile,
}
public abstract class ObjectDesc {
    public readonly uint Id;
    public readonly uint UniqueId;
    public readonly string Name;
    public readonly ClassType Class;
    public ObjectDesc(XElement e, uint id, string name) {
        Id = id;
        UniqueId = Resources.GetNextUniqueId();
        Name = name;
        Class = e.ParseEnum("Class", ClassType.GameObject);
    }
}
