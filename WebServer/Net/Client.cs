using Shared;
using Shared.Redis.Models;
using System.Net.Sockets;
using WebServer.Core;

namespace WebServer.Net;
public sealed class Client(int id, NetHandler handler)
{
    public readonly int Id = id;
    private readonly NetHandler _handler = handler;

    public TcpClient Tcp;
    public DateTime LastMessageTime;
    public TimeSpan DisconnectTime;
    /// <summary>
    /// Ticks one more time to send any packets then disconnects
    /// </summary>
    public bool LateDisconnect = false;
    public RSA Rsa;
    public Account Account;
    public void Begin(Socket socket)
    {
        if (Tcp is not null)
        {
            Tcp.Disconnect();
            Tcp = null;
        }

        Rsa = null;
        Account = null;
        Tcp = new TcpClient(socket);
        LastMessageTime = DateTime.Now;
        DisconnectTime = TimeSpan.FromMilliseconds(1000);

        Tcp.BeginReceive();
    }
    public Task Tick() {
        if (Tcp is null) {
            Disconnect("Tcp is null");
            return Task.CompletedTask;
        }

        if(DateTime.Now - LastMessageTime > DisconnectTime)
            LateDisconnect = true;

        Tcp.Tick(this);

        if (LateDisconnect) {
            LateDisconnect = false;
            Disconnect("DisconnectNextTick");
        }

        return Task.CompletedTask;
    }

    //Handled on Network Thread
    public Task NetworkTick() {
        if (Tcp is null || !Tcp.IsValid()) {
            Disconnect("SocketIsNullOrDisconnected");
            return Task.CompletedTask;
        }

        Tcp.Send();

        return Task.CompletedTask;
    }
    public void Disconnect(string message) {
        SLog.Debug("Client-Disconnect: {0}", args:[message]);
        Tcp?.Disconnect();

        _handler.AddBack(this);
    }
}
