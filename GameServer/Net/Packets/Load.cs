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
        //Add character to world
        if(world == null)
        {
            SLog.Warn("Tried to get World {0} but is null. Returning nexus world", args: [WorldId]);
            //change world to nexus
        }

        client.Character.LastPlayed = DateTime.Now;
        client.Character.FlushAsync();

        client.Player = new Player(client, world.GetNextId(), client.Character.ClassId);
        world.Enter(client.Player, world.GetSpawnPoint());

        client.Tcp.EnqueueSend(new LoadAck(world.Desc.Name, world.Desc.Description, world.Map.Width, world.Map.Height, world.Map.ChunkSizeWidth, world.Map.ChunkSizeHeight, world.Desc.DisplayNames, client.Player.UniqueId));
    }
}
//Basically Map info
public readonly struct LoadAck : ISend {
    public ushort Id => (ushort)S2C.LoadAck;
    public readonly string WorldName = "";
    public readonly string WorldDescription = "";
    public readonly uint Width = 0;
    public readonly uint Height = 0;
    public readonly ushort ChunkWidth = 8;
    public readonly ushort ChunkHeight = 8;
    public readonly bool DisplayNames = false;
    public readonly uint PlayerId;
    public LoadAck(string worldName, string worldDesc, uint width, uint height, ushort chunkWidth, ushort chunkHeight, bool displayNames, uint playerId) {
        WorldName = worldName;
        WorldDescription = worldDesc;
        Width = width;
        Height = height;
        ChunkWidth = chunkWidth;
        ChunkHeight = chunkHeight;
        DisplayNames = displayNames;
    }
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, WorldName);
        w.Write(b, WorldDescription);
        w.Write(b, Width);
        w.Write(b, Height);
        w.Write(b, ChunkWidth);
        w.Write(b, ChunkHeight);
        w.Write(b, DisplayNames);
    }
}
