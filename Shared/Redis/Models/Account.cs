using Shared.Interfaces;
using StackExchange.Redis;
namespace Shared.Redis.Models;
public sealed class Account : RedisObject, IWriteable {
    public uint Id { get => GetValue<uint>("id"); set => SetValue("id", value); }
    public string Name { get => GetValue<string>("name"); set => SetValue("name", value); }
    public uint NextCharId { get => GetValue<uint>("nextCharId"); set => SetValue("nextCharId", value); }
    public uint GuildId { get => GetValue<uint>("guildId"); set => SetValue("guildId", value); }
    public uint[] Alive { get => GetValue<uint[]>("aliveChars"); set => SetValue("aliveChars", value); }
    public Rank Rank { get => (Rank)GetValue<uint>("rank"); set => SetValue("rank", (uint)value); }
    public DateTime RegisteredTime { get => GetValue<DateTime>("registered"); set => SetValue("registered", value); }
    public DateTime LastOnlineTime { get => GetValue<DateTime>("lastOnline"); set => SetValue("lastOnline", value); }
    public DateTime BannedUntilTime { get => GetValue<DateTime>("bannedUntil"); set => SetValue("bannedUntil", value); }
    public bool Banned { get => GetValue<bool>("banned"); set => SetValue("banned", value); }
    public Account(IDatabase db, uint accountId, string field = null, bool isAsync = false)
        : base(db, "account." + accountId, field, isAsync) {
        Id = accountId;

        if (isAsync || field != null)
            return;

        var time = Utils.FromUnixTimestamp(BannedUntilTime.ToUnixTimestamp());
        if (!Banned || time > DateTime.UtcNow)
            return;
        
        Banned = false;
        FlushAsync();
    }
    public void Write(Writer w, Span<byte> buffer) {
        w.Write(buffer, 0); //Version
        WriteVersionZero(w, buffer);
    }
    private void WriteVersionZero(Writer w, Span<byte> b) {
        w.Write(b, Id);
        w.Write(b, Name);
        w.Write(b, NextCharId);
        w.Write(b, GuildId);
        w.Write(b, (ushort)Rank);
        w.Write(b, RegisteredTime.ToBinary());
        w.Write(b, LastOnlineTime.ToBinary());
        w.Write(b, Banned);

        w.Write(b, (ushort)Alive.Length);
        for (int i = 0; i < Alive.Length; i++)
            w.Write(b, Alive[i]);
    }
}
