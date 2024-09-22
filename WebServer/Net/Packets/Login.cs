using Shared;
using Shared.Redis.Models;
using WebServer.Core;
using WebServer.Net.Interfaces;

namespace WebServer.Net.Packets;
public readonly struct Login : IReceive {
    public readonly string Email;
    public readonly string Password;
    public Login(Reader r, Span<byte> b) {
        Email = r.StringShort(b);
        Password = r.StringShort(b);
    }
    public void Handle(Client client) {
        var redis = Application.Instance.Redis;

        var email = client.Rsa.Decrypt(Email);
        SLog.Debug("Login request from {0}");
        if (!redis.TryLogin(email, client.Rsa.Decrypt(Password))) {
            client.Tcp.EnqueueSend(new LoginAck());
            return;
        }

        if(!redis.TryGetAccount(email, out var acc)) {
            client.Tcp.EnqueueSend(new LoginAck());
            return;
        }

        if (acc.Banned) {
            client.Tcp.EnqueueSend(new LoginAck());
            client.LateDisconnect = true;
            return;
        }

        client.Account = acc;
        client.LastMessageTime = DateTime.Now;
        client.Tcp.EnqueueSend(new LoginAck(true, acc));
    }
}
public readonly struct LoginAck(bool success = false, Account account = null) : ISend {
    public ushort Id => (ushort)S2C.LoginAck;
    public readonly bool IsSuccessful = success;
    public readonly Account Account = account;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, IsSuccessful);
        if (IsSuccessful) {
            Account.Write(w, b);
        }
    }
}
