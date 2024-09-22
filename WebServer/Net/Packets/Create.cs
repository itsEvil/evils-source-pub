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
            client.Tcp.EnqueueSend(new Failure("Account is null..."));
            return;
        }

        if(client.Account.Alive.Length >= Account.MaxCharacterSlots) {
            client.Tcp.EnqueueSend(new Failure("Not enough character slots..."));
            return;
        }

        var app = Application.Instance;
        if(!app.Resources.Id2Player.TryGetValue(Id, out var @class)) {
            client.Tcp.EnqueueSend(new Failure("Player class not found..."));
            return;
        }


        var redis = app.Redis;
        var character = redis.CreateCharacter(client.Account, @class);
        client.Tcp.EnqueueSend(new CreateAck(character));
    }
}
public readonly struct CreateAck : ISend {
    public ushort Id => (byte)S2C.CreateAck;
    private readonly Character Character;
    public CreateAck(Character character) {
        Character = character;
    }
    public void Write(Writer w, Span<byte> b) {
        Character.Write(w, b);
    }
}