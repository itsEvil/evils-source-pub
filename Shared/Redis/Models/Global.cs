using Shared.Interfaces;
using StackExchange.Redis;

namespace Shared.Redis.Models;
public sealed class Global(IDatabase db) : RedisObject(db, "globals", null, false), IWriteable
{
    public uint NextAccountId { get => GetValue<uint>("nextAccountId", 0); set => SetValue("nextAccountId", value); }
    public uint NextGuildId { get => GetValue<uint>("nextGuildId", 0); set => SetValue("nextGuildId", value); }
    public uint NextItemId { get => GetValue<uint>("nextItemId", 0); set => SetValue("nextItemId", value); }
    public uint[] News { get => GetValue<uint[]>("news"); set => SetValue("news", value); }
    public void Write(Writer w, Span<byte> buffer) {
        w.Write(buffer, NextAccountId);
        w.Write(buffer, NextGuildId);
        w.Write(buffer, NextItemId);
    }
    public override string ToString() {
        return $"NextAccId: {NextAccountId} | NextGuildId: {NextGuildId} | NextItemId: {NextItemId}";
    }
}
