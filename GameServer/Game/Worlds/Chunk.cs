using Shared;
using Shared.GameData;
using Shared.Interfaces;
using System.Runtime.InteropServices;

namespace GameServer.Game.Worlds;
public sealed class Chunk : IWriteable {
    public readonly uint[] Tiles;
    public readonly byte Width;
    public readonly byte Height;
    public readonly uint X;
    public readonly uint Y;
    public Chunk(uint x, uint y, byte width, byte height, uint initValue = 0) {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Tiles = new uint[Width * Height];

        if (initValue == 0)
            return;

        for(int ix = 0; ix < Tiles.Length; ix++)
        {
            for(int  iy = 0; iy < Height; iy++)
            {
                Tiles[Width * ix + iy] = initValue;
            }
        }
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

        return Tiles[Width * x + y];
    }
    public void Set(uint x, uint y, uint value) {
#if DEBUG
        if (x < 0 || y < 0 || x > Width || y > Height)
            throw new Exception("Chunk get out of bounds");
#endif

        Tiles[Width * x + y] = value;
    }
    public void Write(Writer w, Span<byte> b)
    {
		w.Write(b, Width);
		w.Write(b, Height);
        w.Write(b, X);
        w.Write(b, Y);
        w.Write(b, (ushort)Tiles.Length);
        //Maybe pre generate the bytes from these tiles on creation/change of chunk
        //and just copy them to the array as that might be much faster

        //Or copy them with a unsafe method im sure thats also possible
        for (int i = 0; i < Tiles.Length; i++)
            w.Write(b, Tiles[i]);

    }
    public static int GetSize(Chunk chunk) {
        const int uintSize = sizeof(uint);
        const int byteSize = sizeof(byte);
        const int ushortSize = sizeof(ushort);
        const int total = uintSize + uintSize + byteSize + byteSize + ushortSize; //Chunk + TilesArray
        int tileSize = chunk.Width * chunk.Height * uintSize;

        return total + tileSize;
    }

    public void WriteToDisk(Writer w, Span<byte> b) {
        w.Write(b, X);
        w.Write(b, Y);
        w.Write(b, (ushort)Tiles.Length);
        //Maybe pre generate the bytes from these tiles on creation/change of chunk
        //and just copy them to the array as that might be much faster

        //Or copy them with a unsafe method im sure thats also possible
        for (int i = 0; i < Tiles.Length; i++)
            w.Write(b, Tiles[i]);
    }
    public Chunk(Reader r, Span<byte> b, byte width, byte height) {
        X = r.UInt(b);
        Y = r.UInt(b);
        var len = r.UShort(b);
        Tiles = new uint[len];

        for (int i = 0; i < Tiles.Length; i++)
            Tiles[i] = r.UInt(b);
    }
}
