using GameServer.Game.Worlds;
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
    public override Task Tick()
    {

        return base.Tick();
    }
    protected override void OnEnterWorld(World world)
    {
        OnEnterUpdate(world);
    }

    protected override void OnLeaveWorld(World world)
    {
        OnLeaveUpdate(world);
    }

    protected override void Update() {
        //Send new tick

        //run some logic

        //Send update
        SendUpdate();
    }
}
