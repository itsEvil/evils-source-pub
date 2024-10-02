using Shared;
using GameServer.Net.Interfaces;

namespace GameServer.Net.Packets;
public readonly struct Drops(uint[] ids) : ISend {
    public ushort Id => (ushort)S2C.Drops;
    private readonly uint[] Ids = ids;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, (ushort)Ids.Length);
        var span = Ids.AsSpan();
        for(int i = 0; i < Ids.Length; i++)
            w.Write(b, span[i]);
    }
}

public readonly struct DropsAck : IReceive {
    public DropsAck(Reader r, Span<byte> b) { }
    public void Handle(Client client) { }
}