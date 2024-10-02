using GameServer.Game.Objects;
using Shared;
using Shared.GameData;
using System.Numerics;
namespace GameServer.Game.Worlds;
public class World {
    public readonly Dictionary<uint, Entity> Entities = []; 
    public readonly Dictionary<uint, Player> Players = [];

    private readonly List<Entity> ToAddEntity = [];
    private readonly List<uint> ToRemoveEntity = [];

    private readonly List<Player> ToAddPlayer = [];
    private readonly List<uint> ToRemovePlayer = [];

    private readonly List<Task> EntityUpdates = new List<Task>(256);
    private readonly List<Task> PlayerUpdates = new List<Task>(128);

    public readonly uint Id;
    public readonly Map Map;
    public readonly WorldDesc Desc;
    public World(uint worldId, WorldDesc worldDesc) {
        Id = worldId;
        Desc = worldDesc;

        Map = new Map(1024, 1024, worldDesc.ChunkSize);
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

        //check if there is a wall object here
        //if so return false as they would be no clipping

        return true;
    }

    public Vector2 GetSpawnPoint()
    {
        return new Vector2(0, 0);
    }

    private void Add()
    {
        foreach (var player in ToAddPlayer)
            Players[player.UniqueId] = player;

        foreach (var entity in ToAddEntity)
            Entities[entity.UniqueId] = entity;
    }

    private void Remove() {
        //This might throw if it cant find a entity to remove
        try {
            foreach (var id in ToRemovePlayer)
                Players.Remove(id);

            foreach (var id in ToRemoveEntity)
                Entities.Remove(id);
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

        //Start ticking all of the players and entities
        foreach (var (_, player) in Players)
            PlayerUpdates.Add(player.Tick());

        foreach (var (_, entity) in Entities)
            EntityUpdates.Add(entity.Tick());

        //Await all of the tasks
        //Player updates will likely take longer so we await them first
        await SetNearby();
        await Task.WhenAll(PlayerUpdates);
        await Task.WhenAll(EntityUpdates);
    }

    private Task SetNearby() {
        const float Sight = 15f;
        const float SightSqr = Sight * Sight;

        foreach(var (_, player) in Players) {
            player.NearbyEntities.Clear();

            foreach(var (_, entity) in Entities) {
                if(Vector2.DistanceSquared(player.Position, entity.Position) > SightSqr)
                    continue;
                
                player.NearbyEntities.Add(entity);
            }
        }

        return Task.CompletedTask;
    }

    protected virtual void Update() { }
    private uint NextId = int.MaxValue;
    public uint GetNextId() => NextId++;
}
