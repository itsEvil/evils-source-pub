using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common;
public class WorldDesc(WorldTypes worldType, string worldName) {
    public readonly WorldTypes WorldType = worldType;
    public readonly string WorldName = worldName;
}

public class ObjectDesc(int id, string name) {
    public readonly string Name = name;
    public readonly int Id = id;
    public readonly int MaximumHp = 100;
}

public class PlayerDesc(int id, string name) : ObjectDesc(id, name) {

}