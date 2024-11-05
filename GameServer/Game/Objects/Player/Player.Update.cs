using GameServer.Game.Worlds;
using GameServer.Net;
using GameServer.Net.Packets;
using System.Numerics;

namespace GameServer.Game.Objects;
public partial class Player  {    
    //Each tick its filled with latest nearby entities
    public readonly Queue<Entity> SentEntities = [];

    public readonly Queue<Entity> NewEntities = [];
    public readonly Queue<Entity> ToRemoveEntities = [];


    private bool[] VisibleChunks = [];
    public bool[] ChunkUpdates = [];

    //Do not modify the chunks inside of NewChunks!
    private readonly List<Chunk> NewChunks = [];
    private Chunk LastChunk;
    private void OnEnterUpdate(World world) {
        VisibleChunks = new bool[world.Map.Chunks.Length];
        ChunkUpdates = new bool[world.Map.Chunks.Length];
        //Send first tile data to client


        NewEntities.Enqueue(this);
        
        OnMove();
        SendUpdate();
    }

    private void OnLeaveUpdate(World world) {
        VisibleChunks = [];
        ChunkUpdates = [];
        NewChunks.Clear();
        LastChunk = null;
        ToRemoveEntities.Enqueue(this);
    }

    public void OnMove() {
        var map = World.Map;
        var playerX = (uint)Math.Abs(Position.X);
        var playerY = (uint)Position.Y;
        uint x = (uint)(playerX / map.ChunkWidth);
        uint y = (uint)(playerY / map.ChunkHeight);
		var centerPos = map.ChunkWidth * x + y;
        //var centerPos = x + y * Map.ChunkSize;
        var currentChunk = map.GetChunk(centerPos);
        //Position has not changed enough to send new chunks
        if(currentChunk == LastChunk)
            return;

        LastChunk = currentChunk;
        NewChunks.Clear();

        //Chunks to send
        if (!VisibleChunks[centerPos]) {
            VisibleChunks[centerPos] = true;
            NewChunks.Add(LastChunk);
        }

        //All chunk positions



        //5 center chunks in a plus form
        AddChunk(centerPos);
        AddChunk(map.ChunkWidth * (uint)(((y + (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)(x / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y - (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)(x / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y) / map.ChunkWidth)) + (uint)((x + (1 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y) / map.ChunkWidth)) + (uint)((x - (1 * map.ChunkSizeWidth)) / map.ChunkHeight));

        //1 radius border around the plus
        AddChunk(map.ChunkWidth * (uint)(((y + (2 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y - (2 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x) / map.ChunkHeight));

        AddChunk(map.ChunkWidth * (uint)(((y) / map.ChunkWidth)) + (uint)((x + (2 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y) / map.ChunkWidth)) + (uint)((x - (2 * map.ChunkSizeWidth)) / map.ChunkHeight));

        AddChunk(map.ChunkWidth * (uint)(((y + (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x + (1 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y + (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x - (1 * map.ChunkSizeWidth)) / map.ChunkHeight));

        AddChunk(map.ChunkWidth * (uint)(((y - (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x + (1 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y - (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x - (1 * map.ChunkSizeWidth)) / map.ChunkHeight));

        //Outer radius
        AddChunk(map.ChunkWidth * (uint)(((y - (2 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x - (1 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y - (2 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x + (1 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y + (2 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x + (1 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y + (2 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x - (1 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y - (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x - (2 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y + (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x - (2 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y - (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x + (2 * map.ChunkSizeWidth)) / map.ChunkHeight));
        AddChunk(map.ChunkWidth * (uint)(((y + (1 * map.ChunkSizeHeight)) / map.ChunkWidth)) + (uint)((x + (2 * map.ChunkSizeWidth)) / map.ChunkHeight));

        //Do not modify the chunks inside of NewChunks!
        Client.Tcp.EnqueueSend(new Tiles(NewChunks));
    }
    private void AddChunk(uint idx) {
        if(idx < 0 || idx >= VisibleChunks.Length) 
            return;

        bool visible = VisibleChunks[idx];
        if (!visible || (visible && ChunkUpdates[idx])) {
            VisibleChunks[idx] = true;

            //Chunk updates happen when the world changes a tile
            ChunkUpdates[idx] = false;

            var chunk = World.Map.GetChunk(idx);
            if (chunk == null)
                return;

            NewChunks.Add(chunk);
        }
    }

    private readonly List<ObjectInfo> m_NewEntityInfos = [];
    private readonly List<uint> m_Drops = [];

    private HashSet<Entity> Nearby = [];

    private HashSet<Entity> GetEntitiesFromChunks()
    {
        Nearby.Clear();
        foreach(var chunk in NewChunks)
        {
            foreach(var (_, entity) in chunk.Entities)
            {
                Nearby.Add(entity);
            }
        }
        return Nearby;
    }
    private void SendUpdate()
    {
        //const float Sight = 15f;
        //const float SightSqr = Sight * Sight;

        //var entities = GetEntitiesFromChunks();
        //foreach (var (_, entity) in World.Entities)
        //{
        //    if (Vector2.DistanceSquared(Position, entity.Position) > SightSqr) //To far away
        //    {
        //        if (SentEntities.Contains(entity))
        //        {
        //            ToRemoveEntities.Enqueue(entity);
        //            continue;
        //        }
        //
        //
        //        continue;
        //    }
        //
        //    NewEntities.Enqueue(entity);
        //}

        if (NewEntities.Count != 0)
        {
            m_NewEntityInfos.Clear();
            while (NewEntities.TryDequeue(out var entity))
            {
                m_NewEntityInfos.Add(new ObjectInfo(entity.ObjectId, entity.UniqueId, entity.Position));
                SentEntities.Enqueue(entity);
            }
            
            Client.Tcp.EnqueueSend(new Net.Packets.Objects(m_NewEntityInfos));
        }

        if(ToRemoveEntities.Count != 0)
        {
            m_Drops.Clear();

            while(ToRemoveEntities.TryDequeue(out var entity))
            {
                m_Drops.Add(entity.UniqueId);
                //SentEntities.Remove(entity);
            }

            Client.Tcp.EnqueueSend(new Drops(m_Drops));
        }
    }
}
