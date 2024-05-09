using common;
using game_server.Networking;
using game_server.Core.Entities;
using game_server.Core.Worlds;
using System.Numerics;

namespace game_server.Core;
public class World(int id, CoreManager manager)
{
    public readonly CoreManager Manager = manager;
    private static int NextWorldId = 1;
    public const int NexusId = -1;
    public const int MarketId = -2;

    public readonly List<Task> _tasks = new(256);

    private readonly List<Entity> _toAddEntities = [];
    private readonly List<Entity> _toRemoveEntities = [];

    public readonly Dictionary<int, Entity> Entities = [];
    public readonly Dictionary<int, Entity> Statics = [];
    public readonly Dictionary<int, Player> Players = [];

    public readonly int Id = id;
    public int AliveTimeMS = 0;
    public WorldDesc? Descriptor { get; private set; }
    private Map? _map;

    private ChunkController? EntityChunks;
    private ChunkController? PlayerChunks;

    public virtual void Init(WorldDesc descriptor, Client? client = null) {
        Descriptor = descriptor;
        _map = new Map(50, 50);

        EntityChunks = new ChunkController(descriptor.Width, descriptor.Height);
        PlayerChunks = new ChunkController(descriptor.Width, descriptor.Height);
        
        _toAddEntities.Add(Entity.Resolve(EntityTypes.Entity, Resources.NameToObjectDesc["Pirate"].Id));
    }
    public virtual void EnterWorld(Entity entity, Vector2 position) {
#if DEBUG
        if(entity is null) {
            SLog.Error("Trying to call EnterWorld with a null entity!");
            return;
        }
#endif

        if (!entity.Init()) {
            SLog.Error("Entity::FailedToInit::Type::{0}", entity.ObjectType);
            entity.Dispose();
            return;
        }

        entity.Start = position;
        MoveEntity(entity, position);

        switch (entity)
        {
            case Player:
                Players.Add(entity.Id, (entity as Player)!);
                PlayerChunks?.Insert(entity);
                break;
            case StaticObject:
                Statics.Add(entity.Id, entity);
                break;
            case Decoy:
                Entities.Add(entity.Id, entity);
                PlayerChunks?.Insert(entity);
                break;
            default:
                Entities.Add(entity.Id, entity);
                EntityChunks?.Insert(entity);
                break;
        }

        entity.EnterWorld(this, position);
    }
    public virtual void LeaveWorld(Entity entity) {
#if DEBUG
        if(entity is null) {
            SLog.Error("LeaveWorld::EntityIsNull");
            return;
        }
#endif

        entity.LeaveWorld();

        switch (entity) {
            case Player:
                Players.Remove(entity.Id);
                break;
            case StaticObject:
                Statics.Remove(entity.Id);
                break;
            default:
                Entities.Remove(entity.Id);
                break;
        }

        ChunkController.Remove(entity);
        entity.Dispose();
    }
    public virtual async Task Tick() {

        AliveTimeMS += Time.PerGameTick;

        for(int i = 0; i < _toRemoveEntities.Count; i++) {
            LeaveWorld(_toRemoveEntities[i]);
        }
        _toRemoveEntities.Clear();
        
        for(int i = 0; i < _toAddEntities.Count; i++) {
            EnterWorld(_toAddEntities[i], new Vector2(25, 25));
        }
        _toAddEntities.Clear();

        var chunks = new HashSet<Chunk>();
        foreach (var (_, en) in Players) {
            for (var k = -ChunkController.ActiveRadius; k <= ChunkController.ActiveRadius; k++) {
                for (var j = -ChunkController.ActiveRadius; j <= ChunkController.ActiveRadius; j++) {
                    if (en.CurrentChunk is null)
                        continue;

                    var chunk = EntityChunks?.GetChunk(en.CurrentChunk.X + k, en.CurrentChunk.Y + j);
                    if (chunk != null)
                        chunks.Add(chunk);
                }
            }
        }

        var entities = new HashSet<Entity>();
        entities.UnionWith(Players.Values);
        entities.UnionWith(ChunkController.GetActiveChunks(chunks));
        
        foreach(var player in Players.Values) {
            _tasks.Add(player.SendUpdate());
        }

        await Task.WhenAll(_tasks);

        foreach (var entity in entities) {
            _tasks.Add(entity.Tick());

            entity.HitByProjectile(new Projectile(0, 0, 5, null));
        }

        foreach (var (_, players) in Players) {
            _tasks.Add(players.Tick());
        }

        await Task.WhenAll(_tasks);
        _tasks.Clear();
    }
    public virtual bool IsAllowedAccess(Client client) { return true; }
    public static World Resolve(int worldId, CoreManager manager) {
        if (worldId > 0) {
            return Resolve(WorldTypes.Generic, manager);
        }
        return Resolve((WorldTypes)worldId, manager);
    }
    public static World Resolve(WorldTypes type, CoreManager manager) {
        return type switch {
            WorldTypes.Vault => new Vault(NextWorldId++, manager),
            WorldTypes.Market => new Market(MarketId, manager),
            WorldTypes.Nexus => new Nexus(NexusId, manager),
            WorldTypes.Realm => new Realm(NexusId, manager),
            _ => new World(NextWorldId++, manager),
        };
    }

    public void MoveEntity(Entity entity, Vector2 to, bool ignoreObjects = false)
    {
#if DEBUG
        if (entity is null) {
            SLog.Warn("MoveEntity::TryingToMoveNullEntity");
            return;
        }
#endif
        if(entity.Position != to)
        {
            entity.Position = to;
            entity.UpdateCount++;

            if (entity is StaticObject && !ignoreObjects)
                return;

            var controller = entity is Player || entity is Decoy ? PlayerChunks : EntityChunks;
            controller?.Insert(entity);
        }
    }
}
