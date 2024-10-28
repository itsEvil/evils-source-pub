using GameServer.Game.Worlds;
using GameServer.Net;
using GameServer.Net.Packets;
using Shared.GameData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Objects;
public partial class Player  {    
    //Each tick its filled with latest nearby entities
    public readonly HashSet<Entity> NearbyEntities = [];

    private bool[] VisibleChunks = [];
    public bool[] ChunkUpdates = [];

    //Do not modify the chunks inside of NewChunks!
    private readonly List<Chunk> NewChunks = [];
    private Chunk LastChunk;
    private void InitOnEnter(World world) {
        VisibleChunks = new bool[world.Map.Chunks.Length];
        ChunkUpdates = new bool[world.Map.Chunks.Length];
        //Send first tile data to client
        OnMove();
    }
    public void OnMove() {
        //Figure out if we need to send new tiles to player


        //For each player make a bool[] the same size as the map chunks
        //Each chunk is 8x8 so this will give 16 diagonal distance
        //and 24 horizontal/vertical distance
        //0,0,1,0,0
        //0,1,1,1,0
        //1,1,1,1,1
        //0,1,1,1,0
        //0,0,1,0,0


        var x = (uint)Position.X;
        var y = (uint)Position.Y;

        var map = World.Map;
		var centerPos = map.ChunkSizeWidth * x + y;
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

        //var aboveOne = World.Map.ChunkSize * x + (y + Map.ChunkSize);
        //var aboveTwo = World.Map.ChunkSize * x + (y + Map.DoubleChunkSize);
        //
        //var belowOne = World.Map.ChunkSize * x + (y - Map.ChunkSize);
        //var belowTwo = World.Map.ChunkSize * x + (y - Map.DoubleChunkSize);
        //
        //var leftOne = World.Map.ChunkSize * (x - Map.ChunkSize) + y;
        //var leftTwo = World.Map.ChunkSize * (x - Map.DoubleChunkSize) + y;
        //
        //var rightOne = World.Map.ChunkSize * (x + Map.ChunkSize) + y;
        //var rightTwo = World.Map.ChunkSize * (x + Map.DoubleChunkSize) + y;
        //
        //var upLeftOne = Map.ChunkSize * (x - Map.ChunkSize) + (y + Map.ChunkSize);
        //var upRightOne = Map.ChunkSize * (x + Map.ChunkSize) + (y + Map.ChunkSize);
        //
        //var belowLeftOne = Map.ChunkSize * (x - Map.ChunkSize) + (y - Map.ChunkSize);
        //var belowRightOne = Map.ChunkSize * (x + Map.ChunkSize) + (y - Map.ChunkSize);
        //
        //AddChunk(aboveOne);
        //AddChunk(aboveTwo);
        //AddChunk(belowOne);
        //AddChunk(belowTwo);
        //AddChunk(leftOne);
        //AddChunk(leftTwo);
        //AddChunk(rightOne);
        //AddChunk(rightTwo);
        //AddChunk(upLeftOne);
        //AddChunk(upRightOne);
        //AddChunk(belowLeftOne);
        //AddChunk(belowRightOne);

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
            if(chunk != null)
                NewChunks.Add(chunk);
        }
    }

    private void SendUpdate() {



    }
}
