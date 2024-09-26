using Shared;
using WebServer.Core;
using WebServer.Net.Interfaces;

namespace WebServer.Net.Packets;
public readonly struct Hello : IReceive
{
    private readonly uint Major;
    private readonly uint Minor;
    public Hello(Reader r, Span<byte> b) {
        Major = r.UInt(b);
        Minor = r.UInt(b);
    }
    public void Handle(Client client) {

        //todo
        //show error ui
        if(Major != Application.Instance.Options.MajorVersion) {
            client.Tcp.EnqueueSend(new Failure($"Latest game version: {Application.Instance.Options.MajorVersion}.{Application.Instance.Options.MinorVersion}"));
            client.LateDisconnect = true;
            return;
        }

        //if(Minor != Application.Instance.Options.MinorVersion)
        //{
        //    //Send a chat message?
        //}

        client.LastMessageTime = DateTime.Now;
        client.Account = null;
        client.Rsa = new RSA();
        client.Tcp.EnqueueSend(new HelloAck(client.Rsa.GetPublicKey()));
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