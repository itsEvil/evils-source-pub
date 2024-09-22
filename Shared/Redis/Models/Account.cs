using Shared.Interfaces;
using StackExchange.Redis;
namespace Shared.Redis.Models;
public sealed class Account : RedisObject, IWriteable {
    public const int MaxCharacterSlots = 20;
    /// <summary>
    /// Id of the account
    /// </summary>
    public uint Id { get => GetValue<uint>("id"); set => SetValue("id", value); }
    /// <summary>
    /// Name of the account
    /// </summary>
    public string Name { get => GetValue<string>("name"); set => SetValue("name", value); }
    /// <summary>
    /// Next character Id
    /// </summary>
    public uint NextCharId { get => GetValue<uint>("nextCharId"); set => SetValue("nextCharId", value); }
    /// <summary>
    /// Id of the guild the user is in. Zero means no guild
    /// </summary>
    public uint GuildId { get => GetValue<uint>("guildId"); set => SetValue("guildId", value); }
    /// <summary>
    /// Array of all the alive characters.
    /// </summary>
    public uint[] Alive { get => GetValue<uint[]>("aliveChars"); set => SetValue("aliveChars", value); }
    /// <summary>
    /// Rank of the account
    /// </summary>
    public Rank Rank { get => (Rank)GetValue<uint>("rank"); set => SetValue("rank", (uint)value); }
    /// <summary>
    /// Time they registered the account
    /// </summary>
    public DateTime RegisteredTime { get => GetValue<DateTime>("registered"); set => SetValue("registered", value); }
    /// <summary>
    /// Last time they had any activity on the server
    /// </summary>
    public DateTime LastOnlineTime { get => GetValue<DateTime>("lastOnline"); set => SetValue("lastOnline", value); }
    /// <summary>
    /// Time the user is banned until
    /// </summary>
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
