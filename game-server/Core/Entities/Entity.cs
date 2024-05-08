using common;
using game_server.Core.Logic;
using game_server.Networking;
using System.Numerics;

namespace game_server.Core.Entities;
public enum EntityTypes {
    Entity,
    Container,
    Player,
}
public class Entity {
    public World? Owner = null;
    private static int NextEntityId = 1;
    public readonly int Id;
    public readonly int ObjectType;
    public string Name = "NotSet";

    public readonly Dictionary<StatType, object> Stats = []; //export stats
    public int HP = 0;
    public int MaxHP = 0;
    public Vector2 Position { get; protected set; }

    public Dictionary<int, int>? StateCooldown = [];
    public Dictionary<int, object>? StateObject = []; //Used for things like WanderStates etc.
    public readonly BehaviorModel? Behaviours;
    public State? CurrentState;
    public State? StateEntryCommonRoot;
    public bool StateEntry = true;
    public Entity(int id, int objectType) {
        Id = id;
        ObjectType = objectType;
        Behaviours = BehaviorDb.Resolve(objectType);
    }
    public virtual Task Tick() {
        //SLog.Debug("Entity::{0}::Hp::{1}::MaxHp::{2}", Name, HP, MaxHP);
        Export();

        //Change to distance check when I add chunks
        try {
            CurrentState?.Tick(this);
        } catch (Exception e) {
            SLog.Error(e);
        }

        return Task.CompletedTask;
    }
    public virtual void Export() {
        Stats[StatType.Hp] = HP;
        Stats[StatType.MaxHp] = MaxHP;
    }
    public virtual void EnterWorld(World world, Vector2 position) {
        if(!Resources.IdToObjectDesc.TryGetValue(ObjectType, out ObjectDesc? desc)) {
            SLog.Debug("ObjectType::{0}::NotFoundInResources::UsingPirate", ObjectType);
            desc = Resources.NameToObjectDesc["Pirate"];
        }

        Owner = world;
        Position = position;
        Name = desc.Name;
        HP = desc.MaximumHp;
        MaxHP = desc.MaximumHp;
        
        if (Behaviours is not null) {
            CurrentState = Behaviours.Root;
            CurrentState?.Enter(this);
        }
    }
    public virtual void LeaveWorld() {

    }
    public virtual void Dispose() {
        Owner = null;
        Stats.Clear();
        StateCooldown?.Clear();
        StateObject?.Clear();

        StateCooldown = null;
        StateObject = null;
    }

    public virtual void Death(string killer = "") {
        SLog.Debug("KilledBy::{0}", killer);
        CurrentState?.Death(this);
        Owner?.LeaveWorld(this);
    }
    public bool HitByProjectile(Projectile proj) {
        HP -= proj.Damage;

        var dead = HP <= 0;
        if (dead)
            Death(proj.Owner?.Name ?? "Grim reaper");

        return dead;
    }
    public static Entity Resolve(EntityTypes type, int objectType) {
        return type switch {
            EntityTypes.Player => throw new Exception("Player should not be resolved from Entity.Resolve"),
            EntityTypes.Container => new Container(GetNextId(), objectType),
            _ => new Entity(GetNextId(), objectType),
        };
    }
    public static int GetNextId() {
        var id = NextEntityId;
        NextEntityId++;
        return id;
    }
}
