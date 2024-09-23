using GameServer.Game.Worlds;
using Shared.GameData;
using System.Numerics;

namespace GameServer.Game.Objects;
public class Entity {
    public readonly uint UniqueId;
    public readonly uint ObjectId;

    public bool IsDead = false;
    public bool IsInvulnerable = false;
    public uint MaxHp;
    public uint Hp;

    public World World;
    public Vector2 Position;

    public int OwnerId = -1;

    protected readonly Dictionary<StatType, object> Stats = [];
    public Entity(uint uniqueId, uint objectId) {
        UniqueId = uniqueId;
        ObjectId = objectId;
    }
    private void PreExport() {
        Stats[StatType.Health] = Hp;
        Stats[StatType.MaxHealth] = MaxHp;

        Export(Stats);
    }

    public Task Tick() {
        //Tick behaviours
        //Tick inherited
        PreExport();
        Update();


        return Task.CompletedTask;
    }
    protected virtual void Update() { } 
    protected virtual void Export(Dictionary<StatType, object> stats) { }
    public void Enter(World world) {
        World = world;
        OnEnterWorld(world);
    }
    protected virtual void OnEnterWorld(World world) { }
}
