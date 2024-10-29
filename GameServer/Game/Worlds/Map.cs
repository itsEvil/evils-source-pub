using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Worlds;
public sealed class Map {
    public readonly byte ChunkSizeWidth;
    public readonly byte ChunkSizeHeight;

    public uint Width;
    public uint Height;

    public Chunk[] Chunks = [];
    public readonly uint ChunkWidth;
    public readonly uint ChunkHeight;
    public readonly uint InitValue;
    public Map(uint width, uint height, byte chunkSizeWidth = 8, byte chunkSizeHeight = 8, uint initValue = 0, bool createChunksArray = true) {
        Width = width;
        Height = height;
        ChunkSizeWidth = chunkSizeWidth;
        ChunkSizeHeight = chunkSizeHeight;

        InitValue = initValue;
        
        ChunkWidth = Width / ChunkSizeWidth;
        ChunkHeight = Height / ChunkSizeHeight;

        if(Width % ChunkSizeWidth != 0)
            ChunkWidth += 1;
        if(Height % ChunkSizeWidth != 0)
            ChunkWidth += 1;

        if (!createChunksArray)
            return;

        Chunks = new Chunk[ChunkWidth * ChunkHeight];
        for (uint x = 0; x < ChunkWidth; x++)
            for(uint y = 0; y < ChunkHeight; y++)
                Chunks[ChunkWidth * x + y] = new Chunk(x, y, ChunkSizeWidth, ChunkSizeHeight, initValue);
    }
    public Chunk GetChunk(uint x, uint y) {
        var chunkX = x / ChunkSizeWidth;
        var chunkY = y / ChunkSizeHeight;

        var idx = ChunkWidth * chunkX + chunkY;

#if DEBUG
        if (idx < 0 || idx >= Chunks.Length)
            return null;
#endif
        return Chunks[idx];
    }
    public Chunk GetChunk(uint idx) {
#if DEBUG
        //if (idx < 0 || idx >= Chunks.Length)
        //    return null;
#endif

        return Chunks[idx];
    }
    public Chunk this[uint x, uint y]
    {
        get => GetChunk(x, y);
    }
    public Map Clone() {
        var map = new Map(Width, Height, ChunkSizeWidth, ChunkSizeHeight, InitValue, false)
        {
            Chunks = [.. Chunks]
        };

        return map;
    }
}
