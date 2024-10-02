using Shared;
using GameServer.Net.Interfaces;
using Shared.Interfaces;
using System.Numerics;
using Shared.GameData;
using Shared.Redis.Models;
using GameServer.Game;

namespace GameServer.Net.Packets;
public readonly struct Update(UpdateInfo[] objects) : ISend {
    public ushort Id => (ushort)S2C.Update;
    private readonly UpdateInfo[] Objects = objects;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, (ushort)Objects.Length);
        var span = Objects.AsSpan();
        for (int i = 0; i < Objects.Length; i++)
            span[i].Write(w, b);
    }
}

public readonly struct UpdateAck : IReceive {
    public UpdateAck(Reader r, Span<byte> b) { }
    public void Handle(Client client) { }
}

public sealed class UpdateInfo : IWriteable
{
    public readonly uint UniqueId;
    public readonly Vector2 Position;
    public readonly KeyValuePair<StatType, object>[] Stats = [];
    public UpdateInfo(uint uniqueId, Vector2 position, Dictionary<StatType, object> stats) {
        UniqueId = uniqueId;
        Position = position;
        Stats = [.. stats];
    }
    public UpdateInfo(Reader r, Span<byte> b) {
        UniqueId = r.UInt(b);
        Position = new Vector2(r.Float(b), r.Float(b));

        var len = r.UShort(b);
        Stats = new KeyValuePair<StatType, object>[len];
        var span = Stats.AsSpan();
        for(int i = 0; i < Stats.Length; i++) {
            var key = (StatType)r.UShort(b);
            span[i] = new KeyValuePair<StatType, object>(
                key,
                Read(key, r, b)
            );
        }
    }

    public static object Read(StatType key, Reader r, Span<byte> b) {
        object ret = null;
        switch (key)
        {
            case StatType.Nothing: break;
            case StatType.MaxHealth: ret = r.UInt(b); break;
            case StatType.MaxResource: ret = r.UInt(b); break;
            case StatType.Health: ret = r.UInt(b); break;
            case StatType.Resource: ret = r.UInt(b); break;
            case StatType.Inventory: {
                    var len = r.UShort(b);
                    ItemData[] arr = new ItemData[len];
                    var span = arr.AsSpan();
                    for (int i = 0; i < arr.Length; i++)
                        span[i] = new ItemData(r,b);

                    ret = arr;
                }
                break;
            case StatType.Stats:
                {
                    var len = r.UShort(b);
                    uint[] arr = new uint[len];
                    var span = arr.AsSpan();
                    for (int i = 0; i < arr.Length; i++)
                        span[i] = r.UInt(b);

                    ret = arr;
                }
                break;
            case StatType.Condition: {
                    var len = r.UShort(b); //amount of conditions to read
                    KeyValuePair<ConditionEffect, KeyValuePair<uint, uint>>[] newEffects = 
                        new KeyValuePair<ConditionEffect, KeyValuePair<uint, uint>>[len];
                    for (int i = 0; i < len; i++)
                    {
                        newEffects[i] = new KeyValuePair<ConditionEffect, KeyValuePair<uint, uint>>(
                            (ConditionEffect)r.UShort(b),
                            new KeyValuePair<uint, uint>(
                                r.UShort(b),
                                r.UShort(b)
                        ));
                    }
                    
                    ret = newEffects;
                }
                break;
            case StatType.Name:
                ret = r.StringShort(b);
                break;
        }

        if (ret == null)
            throw new Exception($"Failed to set return value for key '{key}'");

        return ret;
    }

    public void Write(Writer w, Span<byte> b) {
        w.Write(b, UniqueId);
        w.Write(b, Position.X);
        w.Write(b, Position.Y);

        w.Write(b, (ushort)Stats.Length);
        var span = Stats.AsSpan();
        for(int i = 0; i < Stats.Length; i++) {
            var kvp = span[i];
            w.Write(b, (ushort)kvp.Key);
            try
            {
                WriteObject(w, b, kvp.Key, kvp.Value);
            }
            catch (Exception e) {
                SLog.Error(e);
            }
        }
    }
    private static void WriteObject(Writer w, Span<byte> b, StatType key, object data) {
        switch(data) {
            case ulong @ulong: w.Write(b, @ulong); return;
            case long @long: w.Write(b, @long); return;
            case double @double: w.Write(b, @double); return;
            case float @float: w.Write(b, @float); return;
            case KeyValuePair<ConditionEffect, KeyValuePair<uint, uint>>[] condArr: WriteCondition(w, b, condArr); return;
            case ItemData[] itemDataArr: WriteItemData(w, b, itemDataArr); return;
            case ItemData @itemData: itemData.Write(w, b); return;
            case uint[] uintArr: w.WriteArray(b, uintArr); return;
            case int[] intArr: w.WriteArray(b, intArr); return;
            case ushort[] ushortArr: w.WriteArray(b, ushortArr); return;
            case short[] shortArr: w.WriteArray(b, shortArr); return;
            case string @string: w.Write(b, @string); return;
            case ushort @ushort: w.Write(b, @ushort); return;
            case short @short: w.Write(b, @short); return;
            case int @int: w.Write(b, @int); return;
            case uint @uint: w.Write(b, @uint); return;
            default: throw new Exception($"Invalid StatType '{key}' with data: {data}");
        }
    }
    private static void WriteItemData(Writer w, Span<byte> b, ItemData[] data) { 
        w.Write(b, (ushort)data.Length);
        var span = data.AsSpan();
        for(int i = 0; i < span.Length; i++)
            span[i].Write(w, b);
    }

    private static void WriteCondition(Writer w, Span<byte> b, KeyValuePair<ConditionEffect, KeyValuePair<uint, uint>>[] arr)
    {
        w.Write(b, (ushort)arr.Length);
        var span = arr.AsSpan();
        for (int i = 0; i < span.Length; i++) {
            var kvp = span[i];

            w.Write(b, (ushort)kvp.Key); //Effect
            w.Write(b, (ushort)kvp.Value.Key); //Duration MS
            w.Write(b, (ushort)kvp.Value.Value); //Stack count
        }
    }
}
