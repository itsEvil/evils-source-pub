using common;
using common.db;
using game_server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Entities;
public sealed partial class Player(int id, int objectType) : Entity(id, objectType), IInventory {
    public ItemModel[] Inventory { get; set; } = [];
    public Client? Client = null;

    public int MP = 0;
    public int MaxMP = 0;
    public Ranks Rank = Ranks.None;
    public void Init(Client client) {
        Client = client;

        if(Client.Character is not null) {
            Inventory = [.. Client.Character.Inventory];
        }
        if(Client.Account is not null) {
            Name = Client.Account.Name;
            Rank = Client.Account.Rank;
        }


    }
    public override void Export() {
        base.Export();
        Stats[StatType.Mp] = MP;
        Stats[StatType.MaxMp] = MaxMP;
        Stats[StatType.PublicInventory] = new int[4] { Inventory[0].ItemType, Inventory[1].ItemType, Inventory[2].ItemType, Inventory[3].ItemType };
        Stats[StatType.PrivateInventory] = Inventory;
    }
    public void SaveToCharacter() {
        if (Client is null || Client.Character is null) {
            SLog.Debug("SaveToCharacter::{0}::ClientOrCharacterIsNull", Name);
            return;
        }

        Client.Character.Inventory = Inventory;
        Client.Character.FlushAsync();
    }
    public override Task Tick() {


        SendNewTick();
        return base.Tick();
    }
}