using StackExchange.Redis;
namespace Shared.Redis.Models;
public sealed class Account : RedisObject {
    public uint Id = 0;
    public string Email = "";
    public string Name = "";
    public uint NextCharId = 0;
    public uint GuildId = 0;
    public uint[] Alive = [];
    public Rank Rank = Rank.None;
    public DateTime RegisteredTime;
    public DateTime LastOnlineTime;
    public DateTime BannedUntilTime;

    public Account(IDatabase db, uint accountId, string field = null, bool isAsync = false)
        : base(db, "account." + accountId, field, isAsync)
    {
        Id = accountId;

        if (isAsync || field != null)
            return;

        //var time = Utils.FromUnixTimestamp(BannedUntilTime);
        //if (!Banned || BanLiftTime <= -1 || time > DateTime.UtcNow)
        //    return;
        //
        //Banned = false;
        //BanLiftTime = 0;
        //FlushAsync();
    }
    public void Write(Writer w, Span<byte> buffer) {
        w.Write(buffer, Id);
        w.Write(buffer, Name);
        w.Write(buffer, NextCharId);
        w.Write(buffer, GuildId);
        w.Write(buffer, (ushort)Rank);
        w.Write(buffer, RegisteredTime.Ticks);
        w.Write(buffer, LastOnlineTime.Ticks);
        w.Write(buffer, (ushort)Alive.Length);
        for(int i = 0; i < Alive.Length; i++)
            w.Write(buffer, Alive[i]);
    }
}
