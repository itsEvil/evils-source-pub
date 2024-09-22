using GameServer.Net.Interfaces;
using GameServer.Net.Packets;
using Shared;

namespace GameServer.Net;
public static class PacketHandler
{
    public static bool TryGetPacket(ushort id, Reader r, Span<byte> b, out IReceive packet)
    {
        packet = GetPacket(id, r, b);
        if (packet == null)
            return false;
        return true;
    }
    public static IReceive GetPacket(ushort id, Reader r, Span<byte> b)
    {
        return (C2S)id switch
        {
            C2S.Load => new Load(r, b),
            C2S.Login => new Login(r, b),
            C2S.Hello => new Hello(r, b),
            C2S.Failure => new FailureAck(r,b), 
            _ => null,
        };
    }
}
