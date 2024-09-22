using Shared;
using Shared.Redis.Models;
using WebServer.Core;
using WebServer.Net.Interfaces;

namespace WebServer.Net.Packets;
public readonly struct Register : IReceive {
    public readonly string Email;
    public readonly string Password;
    public Register(Reader r, Span<byte> b) {
        Email = r.StringShort(b);
        Password = r.StringShort(b);
    }
    public void Handle(Client client) {

        //Check if account exists
        var email = client.Rsa.Decrypt(Email);
        var password = client.Rsa.Decrypt(Password);
        var app = Application.Instance;

        SLog.Debug("Register: '{0}' '{1}' ", args: [email, password]);

        if (!Utils.IsValidEmail(email)) {
            client.Tcp.EnqueueSend(new RegisterAck(false, "Invalid Email"));
            return;
        }

        if (app.Redis.TryGetAccount(email, out _)) {
            client.Tcp.EnqueueSend(new RegisterAck(false, "Email already in use"));
            return;
        }

        _ = app.Redis.CreateAccount(email, password);

        client.Tcp.EnqueueSend(new RegisterAck(true, "Success"));
    }
}
public readonly struct RegisterAck(bool success, string message) : ISend {
    public ushort Id => (ushort)S2C.RegisterAck;
    public readonly bool IsSuccessful = success;
    public readonly string Message = message;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, IsSuccessful);
        w.Write(b, Message);
    }
}
