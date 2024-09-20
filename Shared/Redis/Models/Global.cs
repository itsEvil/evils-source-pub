using Shared.Interfaces;
using StackExchange.Redis;

namespace Shared.Redis.Models;
public sealed class Global(IDatabase db, string field = null, bool isAsync = false) : RedisObject(db, "globals", field, isAsync), IWriteable
{
    public uint NextAccountId { get => GetValue<uint>("nextAccountId", 0); set => SetValue("nextAccountId", value); }
    public uint NextGuildId { get => GetValue<uint>("nextGuildId", 0); set => SetValue("nextGuildId", value); }
    public uint NextItemId { get => GetValue<uint>("nextItemId", 0); set => SetValue("nextItemId", value); }
    public void Write(Writer w, Span<byte> buffer) {
        w.Write(buffer, NextAccountId);
        w.Write(buffer, NextGuildId);
        w.Write(buffer, NextItemId);
    }
    public override string ToString() {
        return $"NextAccId: {NextAccountId} | NextGuildId: {NextGuildId} | NextItemId: {NextItemId}";
    }
}
