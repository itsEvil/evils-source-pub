namespace GameServer.Game.Worlds;
public sealed class Chunk {
    public readonly uint[] Tiles;
    public readonly byte Width;
    public readonly byte Height;
    public Chunk(byte width, byte height) {
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
}
