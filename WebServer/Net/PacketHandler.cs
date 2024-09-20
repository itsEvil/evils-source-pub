using Shared;
using WebServer.Net.Interfaces;
using WebServer.Net.Packets;

namespace WebServer.Net;
public static class PacketHandler {
    public static bool TryGetPacket(ushort id, Reader r, Span<byte> b, out IReceive packet) {
        packet = GetPacket(id, r, b);
        if(packet == null)
            return false;
        return true;
    }
    public static IReceive GetPacket(ushort id, Reader r, Span<byte> b) {
        return (C2S)id switch
        {
            C2S.Register => new Register(r, b),
            C2S.Login => new Login(r, b),
            C2S.Hello => new Hello(r, b),
            _ => null,
        };
    }
}
