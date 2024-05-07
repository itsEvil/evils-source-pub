using common;
using common.utils;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace game_server.Networking;

public interface IPacketBase {
    public byte Id { get; }
}
public interface IOutPacket : IPacketBase {
    void Write(Span<byte> buffer, ref int ptr);
}
public interface IInPacket : IPacketBase {
    void Handle(Client client);
}

#region OutPackets 
//Example of Out Packet
public readonly struct OutFailure(int errorId) : IOutPacket {
    public byte Id => (byte)OutPacket.Failure;
    public readonly int ErrorId = errorId;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteInt(buffer, ErrorId, ref ptr);
    }
}
public readonly struct NewTick(int tickId, int tickTime, ObjectStats[] stats) : IOutPacket {
    public byte Id => (byte)OutPacket.NewTick;
    public readonly int TickId = tickId;
    public readonly int TickTime = tickTime;
    public readonly ObjectStats[] Stats = stats;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteInt(buffer, TickId, ref ptr);
        PacketUtils.WriteInt(buffer, TickTime, ref ptr);
        PacketUtils.WriteUShort(buffer, (ushort)Stats.Length, ref ptr);
        for(int i = 0; i < Stats.Length; i++) {
            Stats[i].Write(buffer, ref ptr);
        }
    }
}
public readonly struct Update(TileData[] tiles, ObjectDefinition[] defs, int[] drops) : IOutPacket {
    public byte Id => (byte)OutPacket.Update;
    public readonly TileData[] Tiles = tiles;
    public readonly ObjectDefinition[] Defs = defs;
    public readonly int[] Drops = drops;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteUShort(buffer, (ushort)Tiles.Length, ref ptr);
        for (int i = 0; i < Tiles.Length; i++)
            Tiles[i].Write(buffer, ref ptr);

        PacketUtils.WriteUShort(buffer, (ushort)Defs.Length, ref ptr);
        for (int i = 0; i < Defs.Length; i++)
            Defs[i].Write(buffer, ref ptr);

        PacketUtils.WriteUShort(buffer, (ushort)Drops.Length, ref ptr);
        for (int i = 0; i < Drops.Length; i++)
            PacketUtils.WriteInt(buffer, Drops[i], ref ptr);
    }
}

public readonly struct HelloAck(string rsaKey = "") : IOutPacket {
    public byte Id => (byte)OutPacket.HelloAck;
    public readonly string RSAKey = rsaKey;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteString(buffer, RSAKey, ref ptr);
    }
}

#endregion
#region InPackets
//Example of In Packet
public readonly struct InFailure : IInPacket {
    public byte Id => (byte)InPacket.Failure;
    public readonly int ErrorId;
    public InFailure(Span<byte> buffer, ref int ptr, int len) {
        ErrorId = PacketUtils.ReadInt(buffer, ref ptr, len);
    }
    public void Handle(Client client) {

    }
}

public readonly struct Hello : IInPacket {
    public byte Id => (byte)InPacket.Hello;
    public readonly string GameVersion;
    public Hello(Span<byte> buffer, ref int ptr, int len) {
        GameVersion = PacketUtils.ReadString(buffer, ref ptr, len);
    }
    public void Handle(Client client) {
        client.Enqueue(new HelloAck());
    }
}
public readonly struct Login : IInPacket { //Character creation is only done via app server
    public byte Id => (byte)InPacket.Login;
    public readonly string Email;
    public readonly string Password;
    public readonly int WorldId;
    public readonly int CharacterId;
    public Login(Span<byte> buffer, ref int ptr, int len) {
        Email = PacketUtils.ReadString(buffer, ref ptr, len);
        Password = PacketUtils.ReadString(buffer, ref ptr, len);
        WorldId = PacketUtils.ReadInt(buffer, ref ptr, len);
        CharacterId = PacketUtils.ReadInt(buffer, ref ptr, len);
    }
    public void Handle(Client client) {
        //Try login

        //Send mapInfo if true
    }
}
#endregion
