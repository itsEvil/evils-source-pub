using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Logic.Loots;
public class Threshold : List<MobDrop>, IStateChild {
    public Threshold(float threshold, params MobDrop[] children)
    {
        foreach (var child in children)
        {
            child.Threshold = threshold;
            Add(child);
        }
    }
}
