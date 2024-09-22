using Shared;
using WebServer.Net.Interfaces;

namespace WebServer.Net.Packets;
public readonly struct FailureAck : IReceive {
    private readonly string Message;
    public FailureAck(Reader r, Span<byte> b) { 
        Message = r.StringShort(b);
    }
    public void Handle(Client client) {
        client.Disconnect($"FailureAck: {Message}");
    }
}
public readonly struct Failure : ISend {
    public ushort Id => (ushort)S2C.Failure;
    public readonly string Message;
    public Failure(string message = "Unknown") {
        Message = message;
    }
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, Message);
    }
}
