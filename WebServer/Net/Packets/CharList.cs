using Shared;
using Shared.Redis.Models;
using WebServer.Core;
using WebServer.Net.Interfaces;

namespace WebServer.Net.Packets;

//Client requests the character list
public readonly struct CharList : IReceive {
    public CharList(Reader r, Span<byte> b) { }
    public void Handle(Client client) {
        if(client.Account == null || client.Rsa == null) {
            client.Tcp.EnqueueSend(new Failure("Account is null..."));
            return;
        }

        var redis = Application.Instance.Redis;
        var alive = redis.GetAliveCharacters(client.Account);

        client.Tcp.EnqueueSend(new CharListAck(alive));
    }
}

public readonly struct CharListAck(Character[] characters = null) : ISend
{
    public ushort Id => (ushort)S2C.CharListAck;
    private readonly Character[] Characters = characters;
    public void Write(Writer w, Span<byte> b)
    {
        w.Write(b, (ushort)Characters.Length);
        for (int i = 0; i < Characters.Length; i++)
            Characters[i].Write(w, b);
    }
}
