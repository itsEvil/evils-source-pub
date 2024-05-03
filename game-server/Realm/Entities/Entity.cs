using System.Numerics;

namespace game_server.Realm.Entities;
public enum EntityTypes {
    Entity,
    Container,
    Player,
}
public class Entity(int id, int objectType) {
    public World? Owner;
    private static int NextEntityId = 1;
    public readonly int Id = id;
    public readonly int ObjectType = objectType;
    public Vector2 Position { get; protected set; }
    public virtual Task Tick() {
        return Task.CompletedTask;
    }
    public static int GetNextId() {
        var id = NextEntityId;
        NextEntityId++;
        return id;
    }
    public virtual void EnterWorld(World world) {
        Owner = world;
    }
    public virtual void LeaveWorld() {
        Owner = null;
    }
    public static Entity Resolve(EntityTypes type, int objectType) {
        return type switch {
            EntityTypes.Player => throw new Exception("Player should not be resolved from Entity.Resolve"),
            EntityTypes.Container => new Container(GetNextId(), objectType),
            _ => new Entity(GetNextId(), objectType),
        };
    }
}
