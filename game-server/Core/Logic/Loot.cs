using game_server.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Logic;
public class Loot {
    private List<MobDrop> _possibleDrops = [];
    public Loot(params MobDrop[] drops) {
        SetDropData(drops);
    }
    public Loot(IEnumerable<MobDrop> drops) {
        SetDropData(drops);
    }
    private void SetDropData(IEnumerable<MobDrop> drops) {
        _possibleDrops = new List<MobDrop>(drops);
    }
    public void Handle(Enemy enemy, Player killer) {

    }
}
