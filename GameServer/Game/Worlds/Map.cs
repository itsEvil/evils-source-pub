using Shared;
using Shared.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Worlds;
public sealed class Map {

    public readonly Dictionary<Vector2Byte, List<Region>> Regions = []; //Allows for multiple regions on top of eachother

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
        if(Height % ChunkSizeHeight != 0)
            ChunkHeight += 1;

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

    public void AddRegion(Vector2Byte position, Region region)
    {
        if (position.X < 0 || position.X > Width || position.Y < 0 || position.Y > Height)
        {
            SLog.Error("Trying to add region {1} out of bounds: {0}", args: [position.ToString(), region]);
            return;
        }

        if (!Regions.TryGetValue(position, out var regions))
        {
            regions = [];
            Regions[position] = regions;
        }

        regions.Add(region);
    }
    public bool RemoveRegion(Vector2Byte position, Region region)
    {
        if (position.X < 0 || position.X > Width || position.Y < 0 || position.Y > Height)
        {
            SLog.Error("Trying to remove region {1} out of bounds: {0}", args: [position.ToString(), region]);
            return false;
        }

        if (!Regions.TryGetValue(position, out var regions))
        {
            SLog.Debug("Region at {0} not found", args: [position.ToString()]);
            return false;
        }

        return regions.Remove(region);
    }

    public void Write(Writer w, Span<byte> b)
    {
        w.Write(b, Width);
        w.Write(b, Height);
        w.Write(b, ChunkSizeWidth);
        w.Write(b, ChunkSizeHeight);
        w.Write(b, InitValue);

        w.Write(b, Regions.Count);
        foreach(var (pos, regions) in Regions) {
            w.Write(b, pos.X);
            w.Write(b, pos.Y);
            w.Write(b, (ushort)regions.Count);

            var span = CollectionsMarshal.AsSpan(regions);
            for (int i = 0; i < span.Length; i++)
                w.Write(b, (ushort)span[i]);
        }

        for(uint x = 0; x < ChunkWidth; x++)
            for(uint y = 0; y < ChunkHeight; y++)
                Chunks[ChunkWidth * x + y].Write(w, b);
    }
    public Map(Reader r, Span<byte> b) {
        r.Reset(b.Length);

        Width = r.UInt(b);
        Height = r.UInt(b);

        ChunkSizeWidth = r.Byte(b);
        ChunkSizeHeight = r.Byte(b);
        InitValue = r.UInt(b);

        ChunkWidth = Width / ChunkSizeWidth;
        ChunkHeight = Height / ChunkSizeHeight;

        if (Width % ChunkSizeWidth != 0)
            ChunkWidth += 1;
        if (Height % ChunkSizeHeight != 0)
            ChunkHeight += 1;

        Chunks = new Chunk[ChunkWidth * ChunkHeight];
        for (uint x = 0; x < ChunkWidth; x++)
            for (uint y = 0; y < ChunkHeight; y++)
                Chunks[ChunkWidth * x + y] = new Chunk(r, b, ChunkSizeWidth, ChunkSizeHeight);
    }
    public static Span<byte> WriteMapToBuffer(Map map) {
        var w = new Writer();
        w.Reset();

        var array = new byte[CalculateMapBufferSize(map)];

        
        SLog.Debug("Create map data array length: {0}", args: [array.Length]);

        var b = array.AsSpan();

        map.Write(w, b);

        SLog.Debug("Create map data position: {0}", args: [w.Position]);

        return b[0..w.Position];
    }
    private static int CalculateMapBufferSize(Map map)
    {
        const int uintSize = sizeof(uint);
        const int byteSize = sizeof(byte);
        const int ushortSize = sizeof(byte);

        const int defaultValuesSize = uintSize + uintSize + byteSize + byteSize + uintSize; //width, height, chunkWidth, chunkHeight, InitValue

        if (map.Chunks.Length <= 0)
        {
            throw new Exception("Cannot save map without chunks.");
        }

        var chunksLength = map.Chunks.Length;

        var totalChunkSize = chunksLength * Chunk.GetSize(map.Chunks[0]);


        const int regionLocationSize = byteSize + byteSize + ushortSize;
        
        int regionSize = ushortSize;
        foreach(var (_, regions) in map.Regions)
        {
            regionSize += regionLocationSize;
            regionSize += ushortSize * regions.Count;
        }

        return defaultValuesSize + totalChunkSize + regionSize;
    }
}
