using GameServer.Game.Objects;
using Shared;
using Shared.GameData;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace GameServer.Game.Worlds;
public class World : IDisposable
{
    [JsonIgnore] private static readonly World Empty = new(uint.MaxValue, WorldDesc.Empty, true);

    [JsonIgnore] public readonly Dictionary<uint, Entity> Entities = []; 
    [JsonIgnore] public readonly Dictionary<uint, Player> Players = [];

    [JsonIgnore] private readonly List<Entity> ToAddEntity = [];
    [JsonIgnore] private readonly List<uint> ToRemoveEntity = [];

    [JsonIgnore] private readonly List<Player> ToAddPlayer = [];
    [JsonIgnore] private readonly List<uint> ToRemovePlayer = [];

    [JsonIgnore] private readonly List<Task> EntityUpdates = new List<Task>(256);
    [JsonIgnore] private readonly List<Task> PlayerUpdates = new List<Task>(128);
    
    public readonly uint Id;
    public readonly WorldDesc Desc;
    public readonly bool IsEmpty = false;

    public readonly Stopwatch Watch = Stopwatch.StartNew();

    public bool RequiresOxygen = false;

    [JsonIgnore] public Map Map;
    public World(uint worldId, WorldDesc worldDesc, bool isEmpty = false) {
        Id = worldId;
        Desc = worldDesc;
        IsEmpty = isEmpty;
    }

    public virtual void Init(Map map) {
        //Map = new Map(1024, 1024, Desc.ChunkSizeWidth, Desc.ChunkSizeHeight, Desc.DefaultTile);

        Map = map.Clone(
#if DEBUG
            Desc
#endif   
            ); //Clone the original so we don't modify it for other worlds.
    }

    public void Enter(Entity entity, Vector2 at) {
        entity.Position = at;

        switch (entity) {
            case Player player:
            {
                ToAddPlayer.Add(player);
                break;
            }
            default:
            {
                ToAddEntity.Add(entity);
                break;
            }
        }
    }

    public bool ValidatePosition(Vector2 position) {

        if (position.X < 0 || position.Y < 0 || position.X > Map.Width || position.Y > Map.Height)
            return false;

        var chunk = Map.GetChunk((uint)position.X, (uint)position.Y);

        var tile = chunk.Get((uint)position.X, (uint)position.Y);

        //check if there is a wall object here
        //if so return false as they would be no clipping

        return true;
    }

    public Vector2 GetSpawnPoint() {
        //Get a random spawn point from list of spawn points
        if(Map.Regions.TryGetValue(Region.Spawn, out var points)) {
            var point = points[Random.Shared.Next(0, points.Count)];
            return new Vector2(point.X, point.Y);
        }

        //Or spawn in the middle of the map if it doesn't exist
        return new Vector2(Map.Width / 2, Map.Height / 2);
    }

    private void Add()
    {
        foreach (var player in ToAddPlayer) {
            Players[player.UniqueId] = player;
            player.Enter(this);
        }

        foreach (var entity in ToAddEntity) {
            Entities[entity.UniqueId] = entity;
            entity.Enter(this);
        }
    }

    private void Remove() {
        //This might throw if it cant find a entity to remove
        try {
            foreach (var id in ToRemovePlayer)
            {
                if(Players.TryGetValue(id, out var player)) {
                    player.Leave(this);
                }

                Players.Remove(id);
            }

            foreach (var id in ToRemoveEntity) {
                if(Entities.TryGetValue(id, out var entity)) {
                    entity.Leave(this);   
                }
                Entities.Remove(id);
            }
        } 
        catch(Exception e)
        {
            SLog.Error(e);
        }
    }

    public async Task Tick() {
        Update();

        Add();
        Remove();

        PlayerUpdates.Clear();
        EntityUpdates.Clear();

        //Start ticking all of the players and entities
        foreach (var (_, player) in Players)
            PlayerUpdates.Add(player.Tick());
        

        foreach (var (_, entity) in Entities)
            EntityUpdates.Add(entity.Tick());

        //Await all of the tasks
        //Player updates will likely take longer so we start them first
        //await SetNearby();
        await Task.WhenAll(PlayerUpdates);
        await Task.WhenAll(EntityUpdates);
    }

    //private Task SetNearby() {
    //    const float Sight = 15f;
    //    const float SightSqr = Sight * Sight;
    //
    //    foreach(var (_, player) in Players) {
    //
    //        foreach(var (_, entity) in Entities) {
    //
    //            if (Vector2.DistanceSquared(player.Position, entity.Position) > SightSqr) //To far away
    //            {
    //                if (player.SentEntities.Contains(entity))
    //                {
    //                    player.ToRemoveEntities.Enqueue(entity.UniqueId);
    //                    continue;
    //                }
    //
    //
    //                continue;
    //            }
    //
    //            player.NewEntities.Enqueue(entity);
    //        }
    //
    //    }
    //
    //    return Task.CompletedTask;
    //}

    protected virtual void Update() { }
    private uint NextId = int.MaxValue;
    public uint GetNextId() => NextId++;

    public static string ToRedis(World world) {
        return JsonSerializer.Serialize(world, JsonCache.Options);
    }
    public static World FromRedis(string json) {
        return JsonSerializer.Deserialize<World>(json, JsonCache.Options);
    }
    public void Dispose()
    {
        
    }
    public List<Vector2UInt> GetRegionPositions(Region region) {
        if (!Map.Regions.TryGetValue(region, out var locations)) {
            SLog.Error("Failed to find region {0} on map {1}", args: [region, Desc.ToString()]);
            return [];
        }

        return locations;
    }
}
