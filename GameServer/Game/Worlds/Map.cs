using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Worlds;
public sealed class Map {
    private const int ChunkSize = 8;

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
        for (int i = 0; i < Chunks.Length; i++)
            Chunks[i] = new Chunk(ChunkSize, ChunkSize);
    }
    public Chunk GetChunk(uint x, uint y) {
        var chunkX = x / ChunkSize;
        var chunkY = y / ChunkSize;

        //To get tile within chunk??
        //var tileX = x % ChunkSize;
        //var tileY = y % ChunkSize;

        return Chunks[chunkX + chunkY * ChunkWidth];
    }
}
