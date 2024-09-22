using System.Xml.Linq;

namespace Shared.GameData;
public enum ClassType {
    GameObject,
    Item,
    Player,
    Enemy,
    Projectile,
}
public class ObjectDesc {
    public readonly uint Id;
    public readonly string Name;
    public readonly ClassType Class;
    public ObjectDesc(XElement e, uint id, string name) {
        Id = id;
        Name = name;
        Class = e.ParseEnum("Class", ClassType.GameObject);
    }
}
