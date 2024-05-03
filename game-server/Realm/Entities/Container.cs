using common.db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Realm.Entities;
public interface IInventory {
    public ItemModel[] Inventory { get; set; }
}
public class Container(int id, int objectType) : Entity(id, objectType), IInventory {
    public ItemModel[] Inventory { get; set; } = [];
    public int[] Owners = []; //Account Ids
}
