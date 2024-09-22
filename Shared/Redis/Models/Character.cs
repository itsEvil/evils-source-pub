using Shared.Interfaces;

namespace Shared.Redis.Models;
public sealed class Character : RedisObject, IWriteable
{
    public Account Account { get; init; }
    public uint Id = 0;
    public uint ClassId { get => GetValue<uint>("classId", 0); set => SetValue("classId", value); }
    public uint Fame { get => GetValue<uint>("fame", 0); set => SetValue("fame", value); }
    public uint Level { get => GetValue<uint>("level", 0); set => SetValue("level", value); }
    public uint Exp { get => GetValue<uint>("exp", 0); set => SetValue("exp", value); }
    public uint ExpGoal { get => GetValue<uint>("expGoal", 0); set => SetValue("expGoal", value); }
    public DateTime LastPlayed { get => GetValue<DateTime>("lastPlayed"); set => SetValue("lastPlayed", value); }
    public uint[] Stats { get => GetValue<uint[]>("stats"); set => SetValue("stats", value); }
    public uint[] MaxStats { get => GetValue<uint[]>("maxStats"); set => SetValue("maxStats", value); }
    public ItemData[] Inventory { get => GetValue<ItemData[]>("inventory"); set => SetValue("inventory", value); }

    public Character(Account acc, uint charId, bool isAsync = false) : base(acc.Database, $"char.{acc.Id}.{charId}", null, isAsync)
    {
        Account = acc;
        Id = charId;

    }

    public void Write(Writer w, Span<byte> b) {
        w.Write(b, Id);
        w.Write(b, ClassId);
        w.Write(b, Fame);
        w.Write(b, Level);
        w.Write(b, Exp);
        w.Write(b, ExpGoal);
        w.Write(b, LastPlayed.Ticks);

        w.Write(b, (byte)Stats.Length);
        for (int i = 0; i < Stats.Length; i++)
            w.Write(b, Stats[i]);

        w.Write(b, (byte)MaxStats.Length);
        for (int i = 0; i < MaxStats.Length; i++)
            w.Write(b, MaxStats[i]);

        w.Write(b, (byte)Inventory.Length);
        for(int i = 0; i <  Inventory.Length; i++)
            Inventory[i].Write(w, b);
    }
}
