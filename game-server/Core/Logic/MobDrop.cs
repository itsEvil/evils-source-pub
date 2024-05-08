using common.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Logic;
public abstract class MobDrop(float chance, float threshold, int min, int stackAmount) : IStateChild {
    public readonly float Chance = chance;
    public readonly int Min = min;
    public readonly int StackAmount = stackAmount;
    
    public float Threshold = threshold;
    public abstract ItemModel TryObtainItem(float threshold, bool required);
}
