using Shared;
using WebServer.Net.Interfaces;

namespace WebServer.Net.Packets;
public readonly struct Hello : IReceive
{
    private readonly string Major;
    private readonly string Minor;
    public Hello(Reader r, Span<byte> b) {
        Major = r.StringShort(b);
        Minor = r.StringShort(b);
    }
    public void Handle(Client client) {
        client.LastMessageTime = DateTime.Now;


        client.Rsa = new RSA();
        client.Tcp.EnqueueSend(new HelloAck(client.Rsa.GetPublicKey(), "Latest", "Required"));
        //SLog.Debug("Version:[{0},{1}]", args: [Major, Minor]);
    }
}
public readonly struct HelloAck : ISend {
    public ushort Id => (byte)S2C.HelloAck;

    private readonly string RSA;
    private readonly string LatestVersion;
    private readonly string RequiredVersion;
    public HelloAck(string rsa, string latest, string required) {
        RSA = rsa;
        LatestVersion = latest;
        RequiredVersion = required;
    }

    public void Write(Writer w, Span<byte> b) {
        w.Write(b, RSA);
        w.Write(b, LatestVersion);
        w.Write(b, RequiredVersion);
    }
}