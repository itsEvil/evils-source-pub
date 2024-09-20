using Shared;
using Shared.Redis.Models;
using System.Runtime.InteropServices;
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
        //Handle rsa 

        //Check if account exists


        client.Tcp.EnqueueSend(new LoginAck(false, null));
    }
}
public readonly struct LoginAck(bool success, Account account) : ISend {
    public ushort Id => (ushort)S2C.LoginAck;
    public readonly bool IsSuccessful = success;
    public readonly Account Account = account;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, IsSuccessful);
        if (IsSuccessful)
            Account.Write(w, b);
    }
}
