using game_server.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core;

//Credits to Skillys Edu source
//as it was copied from there with a few modifications
public sealed class Chunk(int x, int y)
{
    public HashSet<Entity> Entities = [];
    public readonly int X = x;
    public readonly int Y = y;
    public override int GetHashCode()
    {
        return (Y << 16) ^ X;
    }

    public override bool Equals(object? obj) {
#if DEBUG
        if (obj is null || obj is not Chunk chunk)
            throw new Exception("Invalid object comparison.");
#endif
        return GetHashCode() == chunk.GetHashCode();
    }
}

public sealed class ChunkController
{
    public const int Size = 8;
    public const int ActiveRadius = 32 / Size;
    public readonly Chunk?[,] Chunks;
    public readonly int Width;
    public readonly int Height;

    private readonly int LengthZero;
    private readonly int LengthOne;
    public ChunkController(int width, int height)
    {
        Width = width;
        Height = height;

        Chunks = new Chunk[Convert(Width), Convert(Height)];
        LengthZero = Chunks.GetLength(0);
        LengthOne = Chunks.GetLength(1);

        for(var x = 0; x < Width; x++)
        {
            for(var y = 0; y < Height; y++)
            {
                Chunks[x, y] = new Chunk(x, y);
            }
        }
    }
    public Chunk? GetChunk(int x, int y)
    {
        if (x < 0 || y < 0 || x >= LengthZero || y >= LengthOne)
            return null;
        return Chunks?[x, y] ?? null;
    }

    public static int Convert(float value) => (int)Math.Ceiling(value / Size);

    public void Insert(Entity en)
    {
#if DEBUG
        if (en == null)
            throw new Exception("Entity is undefined.");
#endif
        var nx = Convert(en.Position.X);
        var ny = Convert(en.Position.Y);
        var chunk = Chunks?[nx, ny] ?? null;

        if (en.CurrentChunk != chunk)
        {
            en.CurrentChunk?.Entities.Remove(en);
            en.CurrentChunk = chunk;
            en.CurrentChunk?.Entities.Add(en);
        }
    }
    public static void Remove(Entity en)
    {
#if DEBUG
        if (en == null)
            throw new Exception("Entity is undefined.");
        if (en.CurrentChunk == null)
            throw new Exception("Chunk is undefined.");
        if (!en.CurrentChunk.Entities.Contains(en))
            throw new Exception("Chunk doesn't contain entity.");
#endif
        en.CurrentChunk.Entities.Remove(en);
        en.CurrentChunk = null;
    }

    public List<Entity> HitTest(Vector2 target, float radius)
    {
        if (Chunks is null)
            return [];

        var result = new List<Entity>();
        var size = Convert(radius);
        var beginX = Convert(target.X);
        var beginY = Convert(target.Y);
        var startX = Math.Max(0, beginX - size);
        var startY = Math.Max(0, beginY - size);
        var endX = Math.Min(LengthZero - 1, beginX + size);
        var endY = Math.Min(LengthOne - 1, beginY + size);

        for (var x = startX; x <= endX; x++)
            for (var y = startY; y <= endY; y++) {
                var chunk = Chunks[x, y];
                if (chunk is null)
                    continue;

                foreach (var en in chunk.Entities)
                    if (Vector2.Distance(target, en.Position) < radius)
                        result.Add(en);
            }

        return result;
    }

    /// <summary>
    /// Gets entities that are within a specific radius.
    /// Do not call on entities which did not EnterWorld yet!
    /// </summary>
    /// <param name="target">Position to get nearby chunks from</param>
    /// <param name="types">The ObjectType's that will be filtered out</param>
    /// <param name="radius">All entities within this distance will be returned</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<Entity> HitTest(Vector2 target, List<int> types, float radius)
    {
        var result = new List<Entity>();
        var size = Convert(radius);
        var beginX = Convert(target.X);
        var beginY = Convert(target.Y);
        var startX = Math.Max(0, beginX - size);
        var startY = Math.Max(0, beginY - size);
        var endX = Math.Min(LengthZero - 1, beginX + size);
        var endY = Math.Min(LengthOne - 1, beginY + size);

        for (var x = startX; x <= endX; x++)
            for (var y = startY; y <= endY; y++)
            {
                var chunk = Chunks[x, y];
                if (chunk is null)
                    continue;

                foreach (var en in chunk.Entities)
                {
                    foreach (var type in types)//For each entity check the list of types
                    {
                        if (en.ObjectType != type)
                            continue;

                        if (Vector2.Distance(target, en.Position) < radius)
                            result.Add(en);
                    }
                }
            }
        return result;
    }

    public static HashSet<Entity> GetActiveChunks(HashSet<Chunk> chunks)
    {
        var result = new HashSet<Entity>();
        foreach (var c in chunks)
            result.UnionWith(c.Entities);

        return result;
    }

    public void Dispose() {
        if (Chunks is null) //Must be already diposed
            return;

        for (var w = 0; w < LengthZero; w++)
            for (var h = 0; h < LengthOne; h++) {
                var chunk = Chunks[w, h];
                if (chunk is null)
                    continue;

                chunk.Entities.Clear();
                Chunks[w, h] = null;
            }
        
        //Chunks = null;
    }
}
