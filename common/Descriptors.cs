using common.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace common;
public class WorldDesc(WorldTypes worldType, string worldName) {
    public readonly WorldTypes WorldType = worldType;
    public readonly string WorldName = worldName;
}

public class ObjectDesc {
    public readonly string Name;
    public readonly ObjectClass Class;
    public readonly int Id;
    public readonly int MaximumHp;
    public ObjectDesc(XElement xml, int id, string name, ObjectClass @class = ObjectClass.GameObject) {
        Name = name;
        Id = id;
        MaximumHp = xml.ParseInt("MaximumHp", 100);
        Class = @class;
    }
}

public class PlayerDesc(XElement xml, int id, string name) : ObjectDesc(xml, id, name, ObjectClass.Player) {

}

public class ItemDesc(XElement xml, int id, string name) : ObjectDesc(xml, id, name, ObjectClass.Equipment) {

}

public class ProjectileDesc(XElement xml, int id, string name) : ObjectDesc(xml, id, name, ObjectClass.Projectile) {

}

public class GroundDesc(XElement xml, int id, string name) {
    public readonly int Id = id;
    public readonly string Name = name;
    public readonly bool NoWalk = xml.ParseBool("NoWalk", false);
    public readonly bool Sink = xml.ParseBool("Sink", false);
    public readonly float Speed = xml.ParseFloat("Speed", 1f);
}