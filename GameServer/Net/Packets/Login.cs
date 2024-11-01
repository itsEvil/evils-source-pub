using Shared;
using Shared.Redis.Models;
using GameServer.Net.Interfaces;
using GameServer.Core;

namespace GameServer.Net.Packets;
public readonly struct Login : IReceive {

    public static WorldIds Ids;

    public readonly string Email;
    public readonly string Password;
    public Login(Reader r, Span<byte> b) {
        Email = r.StringShort(b);
        Password = r.StringShort(b);
    }
    public void Handle(Client client) {
        var app = Application.Instance;
        var redis = app.Redis;

        var email = client.Rsa.Decrypt(Email);
        SLog.Debug("Login request from {0}", args: [email]);
        if (!redis.TryLogin(email, client.Rsa.Decrypt(Password))) {
            client.Tcp.EnqueueSend(new Failure("Failed to login..."));
            return;
        }

        if(!redis.TryGetAccount(email, out var acc)) {
            client.Tcp.EnqueueSend(new Failure("Account not found..."));
            return;
        }

        if (acc.Banned) {
            client.Tcp.EnqueueSend(new Failure("Account is banned..."));
            client.LateDisconnect = true;
            return;
        }

        client.Account = acc;
        client.LastMessageTime = DateTime.Now;

        Ids ??= new WorldIds(app.Resources.Name2Worlds["Town"].UniqueId);

        client.Tcp.EnqueueSend(new LoginAck(acc, Ids));
    }
}
public readonly struct LoginAck(Account account, WorldIds worldIds) : ISend {
    public ushort Id => (ushort)S2C.LoginAck;
    public readonly Account Account = account;
    public readonly WorldIds WorldIds = worldIds;
    public void Write(Writer w, Span<byte> b) {
        Account.Write(w, b);
        WorldIds.Write(w, b);
    }
}
