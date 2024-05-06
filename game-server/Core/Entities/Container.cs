using common.db;
using game_server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Entities;
public interface IInventory {
    public ItemModel[] Inventory { get; set; }
}
public class Container(int id, int objectType) : Entity(id, objectType), IInventory {
    public ItemModel[] Inventory { get; set; } = [];
    public int[] Owners = []; //Account Ids
}
