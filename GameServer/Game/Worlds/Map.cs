using GameServer.Core;
using GameServer.Game.Objects;
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

    public readonly Dictionary<Region, List<Vector2UInt>> Regions = [];
    
    //todo add static objects
    public readonly Dictionary<Vector2UInt, Entity> StaticObjects = [];

    public readonly byte ChunkSizeWidth;
    public readonly byte ChunkSizeHeight;

    public uint Width;
    public uint Height;

    public Chunk[] Chunks = [];
    public readonly uint ChunkWidth;
    public readonly uint ChunkHeight;
    public readonly uint InitValue;
#if DEBUG
    public readonly WorldDesc Descriptor;
#endif
    public Map(
#if DEBUG
        WorldDesc desc,
#endif
        uint width, uint height, byte chunkSizeWidth = 8, byte chunkSizeHeight = 8, uint initValue = 0, bool createChunksArray = true) {  
#if DEBUG
        Descriptor = desc;
#endif

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
    public Map Clone(
#if DEBUG
        WorldDesc desc
#endif
        ) {
        var map = new Map(
#if DEBUG
            desc
#endif   
            ,Width, Height, ChunkSizeWidth, ChunkSizeHeight, InitValue, false)
        {
            Chunks = [.. Chunks]
        };

        return map;
    }

    public void AddRegion(Vector2UInt position, Region region)
    {
        if (position.X < 0 || position.X > Width || position.Y < 0 || position.Y > Height)
        {
            SLog.Error("Trying to add region {1} out of bounds: {0}", args: [position.ToString(), region]);
            return;
        }

        if (!Regions.TryGetValue(region, out var positions))
        {
            positions = [];
            Regions[region] = positions;
        }

        positions.Add(position);
    }
    public bool RemoveRegion(Vector2UInt position, Region region)
    {
        if (position.X < 0 || position.X > Width || position.Y < 0 || position.Y > Height)
        {
            SLog.Error("Trying to remove region {1} out of bounds: {0}", args: [position.ToString(), region]);
            return false;
        }

        if (!Regions.TryGetValue(region, out var positions))
        {
            SLog.Debug("Region at {0} not found", args: [position.ToString()]);
            return false;
        }

        if (!positions.Remove(position))
        {
            SLog.Debug("Failed to find position {0} in {1} regions list", args: [position.ToString(),  region]);
            return false;
        }

        return true;
    }

    public bool IsUnblocked(float x, float y)
    {
        var actualX = (uint)Math.Abs(x);
        var actualY = (uint)y;
        var chunk = GetChunk(actualX, actualY);

        if (chunk == null) {
            return false;
        }

        var tileX = actualX % ChunkSizeWidth;
        var tileY = actualY % ChunkSizeHeight;
        var tile = chunk.GetTile(tileX, tileY);

        if(tile <= 0) {
            return false;
        }

        var desc = Application.Instance.Resources.Id2Tile[tile];

        if (desc.NoWalk)
            return false;

        //todo check static object

        return true;
    }

    public void Write(Writer w, Span<byte> b)
    {
        w.Write(b, Width);
        w.Write(b, Height);
        w.Write(b, ChunkSizeWidth);
        w.Write(b, ChunkSizeHeight);
        w.Write(b, InitValue);

        w.Write(b, (ushort)Regions.Count);
        foreach(var (region, positions) in Regions) {
            w.Write(b, (ushort)region);
            w.Write(b, (ushort)positions.Count);

            var span = CollectionsMarshal.AsSpan(positions);
            for (int i = 0; i < span.Length; i++) {
                var item = span[i];
                w.Write(b, item.X);
                w.Write(b, item.Y);
            }
        }

        for(uint x = 0; x < ChunkWidth; x++)
            for(uint y = 0; y < ChunkHeight; y++)
                Chunks[ChunkWidth * x + y].WriteToDisk(w, b);
    }
    public Map(
#if DEBUG
        WorldDesc desc,
#endif
        Reader r, Span<byte> b) {
        r.Reset(b.Length);

#if DEBUG
        Descriptor = desc;
#endif

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

        var regionCount = r.UShort(b);
        for(int i = 0; i < regionCount; i++) {
            Region region = (Region)r.UShort(b);
            var count = r.UShort(b);
            if(count > 0) {
                List<Vector2UInt> positions = [];
                for(int pi = 0; pi < count; pi++)
                    positions.Add(new(r.UInt(b), r.UInt(b)));
                
                Regions[region] = positions;
            }
        }

#if DEBUG
        if(regionCount <= 0)
        {
            SLog.Warn("Loaded map with '{0}' regions by '{1}'", args: [regionCount, Descriptor.Name]);
        }
#endif


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
        const int ushortSize = sizeof(ushort);

        const int defaultValuesSize = uintSize + uintSize + byteSize + byteSize + uintSize; //width, height, chunkWidth, chunkHeight, InitValue

        if (map.Chunks.Length <= 0)
        {
            throw new Exception("Cannot save map without chunks.");
        }

        var totalChunkSize = map.Chunks.Length * Chunk.GetSize(map.Chunks[0]);


        //const int regionLocationSize = uintSize + uintSize + ushortSize;
        const int uintSize2 = (uintSize * 2);
        int regionSize = ushortSize;
        foreach(var (_, positions) in map.Regions)
        {
            regionSize += ushortSize + ushortSize; //Region type and Positions length
            regionSize += uintSize2 * positions.Count;
        }

        return defaultValuesSize + totalChunkSize + regionSize;
    }
}
