using Shared.Interfaces;

namespace Shared.Redis.Models;
public sealed class Character : RedisObject, IWriteable
{
    public Account Account { get; init; }
    public uint Id = 0;
    public uint Type { get => GetValue<uint>("type", 0); set => SetValue("type", value); }
    public ItemData[] Inventory { get => GetValue<ItemData[]>("inventory"); set => SetValue("inventory", value); }
    public uint[] Stats { get => GetValue<uint[]>("stats"); set => SetValue("type", value); }
    public uint Fame { get => GetValue<uint>("fame", 0); set => SetValue("fame", value); }
    public uint Level { get => GetValue<uint>("level", 0); set => SetValue("level", value); }
    public uint Exp { get => GetValue<uint>("exp", 0); set => SetValue("exp", value); }
    public uint ExpGoal { get => GetValue<uint>("expGoal", 0); set => SetValue("expGoal", value); }
    public DateTime LastPlayed { get => GetValue<DateTime>("lastPlayed"); set => SetValue("lastPlayed", value); }

    public Character(Account acc, uint charId, bool isAsync = false) : base(acc.Database, $"char.{acc.Id}.{charId}", null, isAsync)
    {
        Account = acc;
        Id = charId;

    }

    public void Write(Writer w, Span<byte> buffer) {
        w.Write(buffer, Id);
        w.Write(buffer, Type);
        w.Write(buffer, (byte)Inventory.Length);
        for(int i = 0; i <  Inventory.Length; i++)
            Inventory[i].Write(w, buffer);
        
        w.Write(buffer, Fame);
        w.Write(buffer, Level);
        w.Write(buffer, Exp);
        w.Write(buffer, ExpGoal);
        w.Write(buffer, LastPlayed.Ticks);
    }
}
