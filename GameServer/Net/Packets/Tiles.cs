using Shared;
using GameServer.Net.Interfaces;
using Shared.Interfaces;

namespace GameServer.Net.Packets;
public readonly struct TilesAck : IReceive {
    public TilesAck(Reader r, Span<byte> b) { }
    public void Handle(Client client) { }
}
//Basically Map info
public readonly struct Tiles(ChunkData[] data) : ISend {
    public ushort Id => (ushort)S2C.Tiles;
    private readonly ChunkData[] Data = data;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, (ushort)Data.Length);
        for(int i =0; i < Data.Length; i++)
            Data[i].Write(w, b);

    }
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
