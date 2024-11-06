using Shared;
using GameServer.Net.Interfaces;
using Shared.Interfaces;
using System.Numerics;
using System.Runtime.InteropServices.Marshalling;

namespace GameServer.Net.Packets;
public readonly struct Objects(List<ObjectInfo> newObjects) : ISend {
    public ushort Id => (ushort)S2C.Objects;
    private readonly List<ObjectInfo> NewObjects = newObjects;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, (ushort)NewObjects.Count);

        for(int i = 0; i < NewObjects.Count; i++)
            NewObjects[i].Write(w, b);
    }
}

public sealed class ObjectInfo : IWriteable {
    public readonly uint Id; //Object Id from Game data
    public readonly uint UniqueId; //Id reference to this object
    public readonly byte ClassType;
    public readonly Vector2 Position;
    public ObjectInfo(uint id, uint uniqueId, byte classType, Vector2 position) {
        Id = id;
        UniqueId = uniqueId;
        Position = position;
        ClassType = classType;
    }
    public ObjectInfo(Reader r, Span<byte> b) {
        UniqueId = r.UInt(b);
        Id = r.UInt(b);
        Position = new Vector2(r.Float(b), r.Float(b));
        ClassType = r.Byte(b);
    }
    public void Write(Writer w, Span<byte> b)
    {
        w.Write(b, UniqueId);
        w.Write(b, Id);
        w.Write(b, Position.X);
        w.Write(b, Position.Y);
        w.Write(b, ClassType);
    }
}

public readonly struct ObjectsAck : IReceive {
    public ObjectsAck(Reader r, Span<byte> b) { }
    public void Handle(Client client) { }
}