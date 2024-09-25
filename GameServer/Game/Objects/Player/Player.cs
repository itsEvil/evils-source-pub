using GameServer.Net;
using Shared.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Objects;
public partial class Player : Entity {
    private readonly Client Client;
    public readonly string Name;
    public Player(Client client, uint uniqueId, uint objectId) : base(uniqueId, objectId) {
        Client = client;
        Name = client.Account.Name;

        InitInventory();
    }
    protected override void Export(Dictionary<StatType, object> stats) {
        stats[StatType.Name] = Name;
    }
    protected override void Update() {
        //Send new tick

        //run some logic

        //Send update
    }
}
