using Shared;
using Shared.Redis.Models;
using WebServer.Core;
using WebServer.Net.Interfaces;

namespace WebServer.Net.Packets;
public readonly struct Create(Reader r, Span<byte> b) : IReceive
{
    private readonly uint Id = r.UInt(b);
    public void Handle(Client client) {
        //Not logged in!
        if(client.Account == null || client.Rsa == null) {
            client.Tcp.EnqueueSend(new CreateAck());
            return;
        }

        if(client.Account.Alive.Length >= Account.MaxCharacterSlots) {
            client.Tcp.EnqueueSend(new CreateAck());
            return;
        }

        var app = Application.Instance;
        if(!app.Resources.Id2Player.TryGetValue(Id, out var @class)) {
            client.Tcp.EnqueueSend(new CreateAck());
            return;
        }


        var redis = app.Redis;
        var character = redis.CreateCharacter(client.Account, @class);
        client.Tcp.EnqueueSend(new CreateAck(true, character));
    }
}
public readonly struct CreateAck : ISend {
    public ushort Id => (byte)S2C.CreateAck;

    private readonly bool IsSuccess;
    private readonly Character Character;
    public CreateAck(bool success = false, Character character = null) {
        IsSuccess = success;
        Character = character;
    }

    public void Write(Writer w, Span<byte> b) {
        w.Write(b, IsSuccess);
        if (IsSuccess)
            Character.Write(w, b);
    }
}