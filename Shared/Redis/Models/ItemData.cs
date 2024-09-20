using Shared.Interfaces;

namespace Shared.Redis.Models;
public sealed class ItemData : IWriteable {
    public uint Id = 0;
    public uint Type = 0;
    public uint Stacks = 0;
    public ItemData(uint id, uint type, uint stacks) {
        Id = id;
        Type = type;
        Stacks = stacks;
    }
    public void Write(Writer writer, Span<byte> buffer) {
        writer.Write(buffer, Id);
        writer.Write(buffer, Type);
        writer.Write(buffer, Stacks);
    }
}
