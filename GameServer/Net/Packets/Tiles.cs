using Shared;
using GameServer.Net.Interfaces;
using Shared.Interfaces;
using GameServer.Game.Worlds;

namespace GameServer.Net.Packets;
public readonly struct Tiles(List<Chunk> chunks) : ISend {
    public ushort Id => (ushort)S2C.Tiles;
    private readonly List<Chunk> Chunks = chunks;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, (ushort)Chunks.Count);
        for(int i = 0; i < Chunks.Count; i++)
            Chunks[i].Write(w, b);
    }
}
public readonly struct TilesAck : IReceive {
    public TilesAck(Reader r, Span<byte> b) { }
    public void Handle(Client client) { }
}

public sealed class ChunkData(uint x, uint y, uint[] tiles) : IWriteable {
    public readonly uint X = x;
    public readonly uint Y = y;
    public readonly uint[] Tiles = tiles;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, X);
        w.Write(b, Y);
        w.Write(b, (ushort)Tiles.Length);
        for(int i = 0; i < Tiles.Length; i++)
            w.Write(b, Tiles[i]);
    }
}
