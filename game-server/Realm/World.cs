using common;
using game_server.Networking;
using game_server.Realm.Entities;
using game_server.Realm.Worlds;

namespace game_server.Realm;
public class World {
    private static int NextWorldId = 1;
    public const int NexusId = -1;
    public const int MarketId = -2;

    public readonly List<Task> _tasks = new(256);

    private readonly List<Entity> _toAddEntities = [];
    private readonly List<Entity> _toRemoveEntities = [];

    public readonly Dictionary<int, Entity> Entities = [];
    public readonly Dictionary<int, Player> Players = [];

    public readonly int Id;
    public readonly DateTime TimeUntilLeave = DateTime.Now + TimeSpan.FromSeconds(30);
    public WorldDesc Descriptor { get; private set; }
    private Map _map;
    public World(int id) {
        Id = id;
    }
    public virtual void Init(WorldDesc descriptor, Client? client = null) {
        Descriptor = descriptor;
        _map = new Map(50, 50);

        _toAddEntities.Add(Entity.Resolve(EntityTypes.Entity, 1));
    }
    public virtual void EnterWorld(Entity entity) {
        entity.EnterWorld(this);
        if(entity is Player player) {
            Players.Add(player.Id, player);
        } else {
            Entities.Add(entity.Id, entity);
        }
    }
    public virtual void LeaveWorld(Entity entity) {
        entity.LeaveWorld();
        if (entity is Player player) {
            Players.Remove(player.Id);
        }
        else {
            Entities.Remove(entity.Id);
        }
        entity.Dispose();
    }
    public virtual async Task Tick() {
        for(int i = 0; i < _toRemoveEntities.Count; i++) {
            LeaveWorld(_toRemoveEntities[i]);
        }
        _toRemoveEntities.Clear();
        
        for(int i = 0; i < _toAddEntities.Count; i++) {
            EnterWorld(_toAddEntities[i]);
        }
        _toAddEntities.Clear();

        foreach(var (_, entity) in Entities) {
            _tasks.Add(entity.Tick());

            entity.HitByProjectile(new Projectile(0, 0, 1, null));
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
            WorldTypes.Realm => new Worlds.Realm(NexusId),
            _ => new World(NextWorldId++),
        };
    }
}
