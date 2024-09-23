using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Worlds;
public sealed class Map {
    public const int ChunkSize = 8;
    public const int DoubleChunkSize = ChunkSize * 2;

    public uint Width;
    public uint Height;

    public Chunk[] Chunks = [];
    private readonly uint ChunkWidth;
    private readonly uint ChunkHeight;

    public Map(uint width, uint height) {
        Width = width;
        Height = height;

        ChunkWidth = Width / ChunkSize;
        ChunkHeight = Height / ChunkSize;

        if(Width % ChunkSize != 0)
            ChunkWidth += 1;
        if(Height % ChunkSize != 0)
            ChunkWidth += 1;
        
        Chunks = new Chunk[ChunkWidth * ChunkHeight];
        for (uint x = 0; x < ChunkWidth; x++)
            for(uint y = 0; y < ChunkHeight; y++)
                Chunks[x + y * ChunkWidth] = new Chunk(x,y, ChunkSize, ChunkSize);
    }
    public Chunk GetChunk(uint x, uint y) {
        var chunkX = x / ChunkSize;
        var chunkY = y / ChunkSize;

        //To get tile within chunk??
        //var tileX = x % ChunkSize;
        //var tileY = y % ChunkSize;

        var idx = chunkX + chunkY * ChunkWidth;

        //Add the range check yourself if its nessesary 
#if DEBUG
        //if (idx < 0 || idx >= Chunks.Length)
        //    return null;
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
}
