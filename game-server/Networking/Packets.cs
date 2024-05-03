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

//Example of Out Packet
public readonly struct OutFailure(int errorId) : IOutPacket {
    public byte Id => (byte)OutPacket.Failure;
    public readonly int ErrorId = errorId;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteInt(buffer, ErrorId, ref ptr);
    }
}

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
