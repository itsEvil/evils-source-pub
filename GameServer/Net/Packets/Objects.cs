using Shared;
using GameServer.Net.Interfaces;
using Shared.Interfaces;
using System.Numerics;

namespace GameServer.Net.Packets;
public readonly struct Objects(ObjectInfo[] newObjects) : ISend {
    public ushort Id => (ushort)S2C.Objects;
    private readonly ObjectInfo[] NewObjects = newObjects;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, (ushort)NewObjects.Length);
        var span = NewObjects.AsSpan();
        for(int i = 0; i < NewObjects.Length; i++)
            span[i].Write(w, b);
    }
}

public sealed class ObjectInfo : IWriteable {
    public readonly uint Id;
    public readonly uint UniqueId;
    public readonly Vector2 Position;
    public ObjectInfo(uint id, uint uniqueId, Vector2 position) {
        Id = id;
        UniqueId = uniqueId;
        Position = position;
    }
    public ObjectInfo(Reader r, Span<byte> b) {
        UniqueId = r.UInt(b);
        Id = r.UInt(b);
        Position = new Vector2(r.Float(b), r.Float(b));
    }
    public void Write(Writer w, Span<byte> b)
    {
        w.Write(b, UniqueId);
        w.Write(b, Id);
        w.Write(b, Position.X);
        w.Write(b, Position.Y);
    }
}

public readonly struct ObjectsAck : IReceive {
    public ObjectsAck(Reader r, Span<byte> b) { }
    public void Handle(Client client) { }
}