using StackExchange.Redis;

namespace Shared.Redis.Models;
public sealed class Global : RedisObject {
    public Global(IDatabase db, string field = null, bool isAsync = false) : base(db, "globals", field, isAsync) {
        if(NextAccountId == 0)
            NextAccountId = 0;
        
        if(NextGuildId == 0)
            NextGuildId = 0;

        if (NextItemId == 0)
            NextItemId = 0;
    }

    public uint NextAccountId { get => GetValue<uint>("nextAccountId"); set => SetValue("nextAccountId", value); }
    public uint NextGuildId { get => GetValue<uint>("nextGuildId"); set => SetValue("nextGuildId", value); }
    public uint NextItemId { get => GetValue<uint>("nextItemId"); set => SetValue("nextItemId", value); }

    public void Write(Writer w, Span<byte> buffer) {
        w.Write(buffer, NextAccountId);
        w.Write(buffer, NextGuildId);
        w.Write(buffer, NextItemId);
    }
    public override string ToString() {
        return $"NextAccId: {NextAccountId} | NextGuildId: {NextGuildId} | NextItemId: {NextItemId}";
    }
}
