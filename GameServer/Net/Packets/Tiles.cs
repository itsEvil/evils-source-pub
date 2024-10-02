using Shared;
using GameServer.Net.Interfaces;
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