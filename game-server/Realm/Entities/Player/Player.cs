using common;
using common.db;
using game_server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Realm.Entities;
public sealed class Player(int id, int objectType) : Entity(id, objectType), IInventory {
    public ItemModel[] Inventory { get; set; } = [];
    public Client? Client = null;

    public SyncedVariable<int> MP = SyncedVariable<int>.EmptyInt;
    public SyncedVariable<int> MaxMP = SyncedVariable<int>.EmptyInt;
    public void Init(Client client) {
        Client = client;

        if(Client.Character is not null) {

        }

        if(Client.Account is not null) {
            Name = Client.Account.Name;
        }


    }
    public override void Export() {
        base.Export();
        Stats[StatType.Mp] = MP.Value;
        Stats[StatType.MaxMp] = MaxMP.Value;
    }
    public void SaveToCharacter() {
        if (Client is null || Client.Character is null) {
            SLog.Debug("SaveToCharacter::{0}::ClientOrCharacterIsNull", Name);
            return;
        }

        Client.Character.Inventory = Inventory;

    }
}