using Shared;
using WebServer.Core;
using WebServer.Net.Interfaces;

namespace WebServer.Net.Packets;
public readonly struct Register : IReceive {
    public readonly string Email;
    public readonly string Password;
    public readonly string Name;
    public Register(Reader r, Span<byte> b) {
        Email = r.StringShort(b);
        Password = r.StringShort(b);
        Name = r.StringShort(b);
    }
    public void Handle(Client client) {

        if(string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Name)) {
            client.Tcp.EnqueueSend(new Failure("Invalid register data"));
            return;
        }
        //Check if account exists
        var email = client.Rsa.Decrypt(Email);
        var password = client.Rsa.Decrypt(Password);
        var app = Application.Instance;


        SLog.Debug("Register: '{0}' '{1}' ", args: [email, password]);

        if (!Utils.IsValidEmail(email)) {
            client.Tcp.EnqueueSend(new Failure("Invalid Email"));
            return;
        }

        if (app.Redis.TryGetAccount(email, out _)) {
            client.Tcp.EnqueueSend(new Failure("Email already in use"));
            return;
        }


        if (app.Redis.IsNameTaken(Name)) {
            client.Tcp.EnqueueSend(new Failure("Name is taken!"));
            return;
        }

        _ = app.Redis.CreateAccount(email, password, Name);

        client.Tcp.EnqueueSend(new RegisterAck("Success"));
    }
}
public readonly struct RegisterAck(string message) : ISend {
    public ushort Id => (ushort)S2C.RegisterAck;
    public readonly string Message = message;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, Message);
    }
}
