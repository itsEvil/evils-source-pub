using Shared;
using System.Net.Sockets;
using WebServer.Core;

namespace WebServer.Net;
public sealed class Client {

    public TcpClient Tcp;
    public readonly int Id;

    public DateTime LastMessageTime;
    public DateTime DisconnectTime;

    private readonly NetHandler _handler;
    public bool ForceDisconnect = false;

    public RSA Rsa;

    public Client(int id, NetHandler handler) {
        _handler = handler;
        Id = id;
    }
    public void Begin(Socket socket)
    {
        if (Tcp is not null)
        {
            Tcp.Disconnect();
            Tcp = null;
        }

        Tcp = new Net.TcpClient(socket);
        LastMessageTime = DateTime.Now;
        DisconnectTime = DateTime.Now + TimeSpan.FromMilliseconds(1000);

        Tcp.BeginReceive();
    }
    public Task Tick() {
        if (Tcp is null || ForceDisconnect)
            return Task.CompletedTask;

        Tcp.Tick(this);

        return Task.CompletedTask;
    }

    //Handled on Network Thread
    public Task NetworkTick() {
        //if (Tcp is null || ForceDisconnect)
        //{
        //    Tcp.Disconnect();
        //    //Tcp.Disconnect("SocketIsNullOrDisconnected");
        //    return Task.CompletedTask;
        //}
        //
        //if (DisconnectTime < DateTime.Now) {
        //    Tcp.Disconnect();
        //    //Disconnect($"TookTooLongToRespond::DCTime:{DisconnectTime}::Now:{DateTime.Now}");
        //    return Task.CompletedTask;
        //}

        if (Tcp is null || !Tcp.IsValid()) {
            Disconnect("SocketIsNullOrDisconnected");
            return Task.CompletedTask;
        }

        Tcp.Send();

        return Task.CompletedTask;
    }
    public void Disconnect(string message) {
        SLog.Debug("Client-Disconnect: {0}", args:[message]);
        Tcp.Disconnect();

        _handler.AddBack(this);
    }
}
