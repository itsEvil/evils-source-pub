using common;
using game_server.Networking;
using game_server.Realm.Entities;
using game_server.Realm.Worlds;

namespace game_server.Realm;

public enum WorldTypes { //Not linked to world Id's
    Generic,
    Nexus = -1,
    Vault = -2,
    Market = -3,
}
public class World {
    private static int NextWorldId = 1;
    public const int NexusId = -1;
    public const int MarketId = -2;

    public readonly List<Task> _tasks = new(256);

    private readonly List<Entity> _toAddEntities = [];
    private readonly List<int> _toRemoveEntities = [];

    public readonly Dictionary<int, Entity> Entities = [];
    public readonly Dictionary<int, Player> Players = [];

    public readonly int Id;
    public WorldDesc Descriptor { get; private set; }
    private Map _map;
    public World(int id) {
        Id = id;
    }
    public virtual void Init(WorldDesc descriptor, Client? client = null) {
        Descriptor = descriptor;
        _map = new Map(50, 50);
    }
    public virtual void EnterWorld(Entity entity) {
        if(entity is Player player) {
            Players.Add(player.Id, player);
        } else {
            Entities.Add(entity.Id, entity);
        }
        entity.EnterWorld(this);
    }
    public virtual void LeaveWorld(Entity entity) {
        entity.LeaveWorld();
        if (entity is Player player) {
            Players.Remove(player.Id);
        }
        else {
            Entities.Remove(entity.Id);
        }
    }
    public virtual async Task Tick() {
        foreach(var (_, entity) in Entities) {
            _tasks.Add(entity.Tick());
        }

        foreach (var (_, players) in Players) {
            _tasks.Add(players.Tick());
        }

        await Task.WhenAll(_tasks);
        _tasks.Clear();
    }
    public virtual bool IsAllowedAccess(Client client) { return true; }
    public static World Resolve(int worldId) {
        if (worldId > 0) {
            return Resolve(WorldTypes.Generic);
        }
        return Resolve((WorldTypes)worldId);
    }
    public static World Resolve(WorldTypes type) {
        return type switch {
            WorldTypes.Vault => new Vault(NextWorldId++),
            WorldTypes.Market => new Market(MarketId),
            WorldTypes.Nexus => new Nexus(NexusId),
            _ => new World(NextWorldId++),
        };
    }
}
