using Shared;
using Shared.Redis.Models;
using System.Xml.Linq;
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

        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
        {
            client.Tcp.EnqueueSend(new Failure("Invalid login data"));
            return;
        }

        if (client.Account != null) {
            client.Tcp.EnqueueSend(new Failure("Already logged in..."));
            return;
        }

        var email = client.Rsa.Decrypt(Email);
        SLog.Debug("Login request from {0}", args: [client.Tcp.Address]);
        if (!redis.TryLogin(email, client.Rsa.Decrypt(Password))) {
            client.Tcp.EnqueueSend(new Failure("Failed to login..."));
            return;
        }

        if(!redis.TryGetAccount(email, out var acc)) {
            client.Tcp.EnqueueSend(new Failure("Account does not exist..."));
            return;
        }

        if (acc.Banned) {
            client.Tcp.EnqueueSend(new Failure("Account is banned..."));
            client.LateDisconnect = true;
            return;
        }

        SLog.Debug("Sending back LoginAck to {0}", args: [acc.Name]);
        client.Account = acc;
        client.LastMessageTime = DateTime.Now;
        client.Tcp.EnqueueSend(new LoginAck(acc, redis.GameServers));
    }
}
public readonly struct LoginAck(Account account, Server[] servers) : ISend {
    public ushort Id => (ushort)S2C.LoginAck;
    public readonly Account Account = account;
    public readonly Server[] Servers = servers;
    public void Write(Writer w, Span<byte> b) {
        Account.Write(w, b);
        w.Write(b, (ushort)Servers.Length);
        foreach (Server server in Servers)
            server.Write(w, b);
    }
}
