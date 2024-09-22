using Shared;
using Shared.Redis.Models;
using GameServer.Net.Interfaces;
using GameServer.Core;
using GameServer.Game.Objects;
using GameServer.Game.Worlds;

namespace GameServer.Net.Packets;
public readonly struct Load : IReceive {
    public readonly uint CharacterId;
    public readonly uint WorldId;
    public Load(Reader r, Span<byte> b) {
        CharacterId = r.UInt(b);
        WorldId = r.UInt(b);
    }
    public void Handle(Client client) {

        if(client.Account == null) {
            client.Tcp.EnqueueSend(new Failure("Account is null..."));
            return;
        }

        //Reload alive ids 
        client.Account.Reload("alive");
        if (!client.Account.Alive.Contains(CharacterId)) {
            client.Tcp.EnqueueSend(new Failure("Character not found..."));
            return;
        }

        client.Character = new Character(client.Account, CharacterId);

        World world = Application.Instance.WorldManager.Get(WorldId);
        //Put any fixes on character or inventories here

        client.Character.FlushAsync();

        client.Player = new Player(client, world.GetNextId(), client.Character.ClassId);
    }
}
//Basically Map info
public readonly struct LoadAck : ISend {
    public ushort Id => (ushort)S2C.LoadAck;
    public readonly string WorldName = "";
    public readonly string WorldDescription = "";
    public readonly int Width = 0;
    public readonly int Height = 0;
    public readonly bool DisplayNames = false;
    public LoadAck(string worldName, string worldDesc, int width, int height, bool displayNames) {
        WorldName = worldName;
        WorldDescription = worldDesc;
        Width = width;
        Height = height;
        DisplayNames = displayNames;
    }
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, WorldName);
        w.Write(b, WorldDescription);
        w.Write(b, Width);
        w.Write(b, Height);
        w.Write(b, DisplayNames);
    }
}
