using Shared.Interfaces;

namespace Shared.Redis.Models;
public sealed class ItemData : IWriteable {
    public readonly uint UniqueId = 0;
    public readonly uint Id = 0;
    public uint Stacks = 0;

    public readonly bool IsEmpty;
    public ItemData(uint unqiueId = 0, uint id = 0, uint stacks = 0) {
        UniqueId = unqiueId;
        Id = id;
        Stacks = stacks;
        IsEmpty = UniqueId == 0 && Id == 0;
    }
    public ItemData(Reader r, Span<byte> b) {
        UniqueId = r.UInt(b);
        Id = r.UInt(b);
        Stacks = r.UInt(b);
        IsEmpty = UniqueId == 0 && Id == 0;
    }
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, UniqueId);
        w.Write(b, Id);
        w.Write(b, Stacks);
    }
}
