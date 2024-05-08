using common.db;
using game_server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Entities;
public class Enemy(int id, int objectType) : Entity(id, objectType) {
    public override void Death(string killer = "") {
        base.Death(killer);


    }
}
