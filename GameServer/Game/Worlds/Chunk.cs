using Shared;
using Shared.Interfaces;

namespace GameServer.Game.Worlds;
public sealed class Chunk : IWriteable {
    public readonly uint[] Tiles;
    public readonly byte Width;
    public readonly byte Height;
    public readonly uint X;
    public readonly uint Y;
    public Chunk(uint x, uint y, byte width, byte height) {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Tiles = new uint[Width * Height];
    }
    public uint this[uint x, uint y] {
        get => Get(x,y);
        set => Set(x,y, value);
    }
    public uint Get(uint x, uint y) {
#if DEBUG
        if (x < 0 || y < 0 || x > Width || y > Height)
            throw new Exception("Chunk get out of bounds");
#endif

        return Tiles[x + y * Width];
    }
    public void Set(uint x, uint y, uint value) {
#if DEBUG
        if (x < 0 || y < 0 || x > Width || y > Height)
            throw new Exception("Chunk get out of bounds");
#endif

        Tiles[x + y * Width] = value;
    }

    public void Write(Writer w, Span<byte> b)
    {
        w.Write(b, X);
        w.Write(b, Y);
        w.Write(b, (ushort)Tiles.Length);

        //Maybe pre generate the bytes from these tiles on creation/change of chunk
        //and just copy them to the array as that might be much faster

        //Or copy them with a unsafe method im sure thats also possible
        for (int i = 0; i < Tiles.Length; i++)
            w.Write(b, Tiles[i]);
    }
}
