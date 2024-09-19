using Shared;

namespace WebServer.Net.Packets;
public readonly struct Hello : IReceive
{
    private readonly string Major;
    private readonly string Minor;
    public Hello(Reader reader, Span<byte> buffer) {
        Major = reader.StringShort(buffer);
        Minor = reader.StringShort(buffer);
    }
    public void Handle(Client client) {
        SLog.Debug("Version:[{0},{1}]", args: [Major, Minor]);
        client.Tcp.EnqueueSend(new HelloAck("RSAstuffHere", "Latest", "Required"));
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

    public void Write(Writer writer, Span<byte> buffer) {
        writer.Write(buffer, RSA);
        writer.Write(buffer, LatestVersion);
        writer.Write(buffer, RequiredVersion);
    }
}