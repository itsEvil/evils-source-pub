using Shared;
using GameServer.Net.Interfaces;

namespace GameServer.Net.Packets;
public readonly struct Drops(List<uint> ids) : ISend {
    public ushort Id => (ushort)S2C.Drops;
    private readonly List<uint> Ids = ids;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, (ushort)Ids.Count);

        //Change to CollectionMarshal.AsSpan()?
        for(int i = 0; i < Ids.Count; i++)
            w.Write(b, Ids[i]);
    }
}

public readonly struct DropsAck : IReceive {
    public DropsAck(Reader r, Span<byte> b) { }
    public void Handle(Client client) { }
}