using Shared;
using GameServer.Net.Interfaces;

namespace GameServer.Net.Packets;
public readonly struct Hello : IReceive
{
    private readonly uint Major;
    private readonly uint Minor;
    public Hello(Reader r, Span<byte> b) {
        Major = r.UInt(b);
        Minor = r.UInt(b);
    }
    public void Handle(Client client) {
        client.LastMessageTime = DateTime.Now;


        client.Rsa = new RSA();
        client.Tcp.EnqueueSend(new HelloAck(client.Rsa.GetPublicKey()));
        //SLog.Debug("Version:[{0},{1}]", args: [Major, Minor]);
    }
}
public readonly struct HelloAck : ISend {
    public ushort Id => (byte)S2C.HelloAck;

    private readonly string RSA;
    public HelloAck(string rsa) {
        RSA = rsa;
    }
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, RSA);
    }
}