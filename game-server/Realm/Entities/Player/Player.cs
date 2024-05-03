using common.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Realm.Entities;
public sealed class Player(int id, int objectType) : Entity(id, objectType), IInventory {
    public ItemModel[] Inventory { get; set; } = [];
}