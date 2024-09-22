using GameServer.Net;
using Shared.GameData;
using Shared.Redis.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Objects;
public partial class Player {
    private ItemData[] Inventory = [];
    private void InitInventory() {
        Inventory = [.. Client.Character.Inventory];
        Stats[StatType.Inventory] = Inventory; // Never have to update this key as it its a reference to the array!
    }
}
