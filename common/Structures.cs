using common.db;
using common.utils;
using System.Numerics;

namespace common;

public readonly struct TileData(ushort x, ushort y, int tileType) {
    public readonly ushort X = x;
    public readonly ushort Y = y;
    public readonly int TileType = tileType;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteUShort(buffer, X, ref ptr);
        PacketUtils.WriteUShort(buffer, Y, ref ptr);
        PacketUtils.WriteInt(buffer, TileType, ref ptr);
    }
}
public readonly struct ObjectDefinition(int objectType, ObjectStats stats) {
    public readonly int ObjectType = objectType;
    public readonly ObjectStats Stats = stats;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteInt(buffer, ObjectType, ref ptr);
        Stats.Write(buffer, ref ptr);
    }
}
public readonly struct ObjectStats(int id, Vector2 position, KeyValuePair<StatType, object>[] stats) {
    public readonly int Id = id;
    public readonly Vector2 Position = position;
    public readonly KeyValuePair<StatType, object>[] Stats = stats;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteInt(buffer, Id, ref ptr);
        PacketUtils.WriteFloat(buffer, Position.X, ref ptr);
        PacketUtils.WriteFloat(buffer, Position.Y, ref ptr);

        PacketUtils.WriteByte(buffer, (byte)Stats.Length, ref ptr);
        foreach(var (key, value) in Stats) {
            PacketUtils.WriteByte(buffer, (byte)key, ref ptr);
            WriteStat(key, value, buffer, ref ptr);
        }
    }

    public static void WriteStat(StatType key, object value, Span<byte> buffer, ref int ptr) {
        switch (value) {
            default: SLog.Debug("FailedToWriteStat::{0}", key); return;
            case int: PacketUtils.WriteInt(buffer, (int)value, ref ptr); return;
            case string: PacketUtils.WriteString(buffer, (string)value, ref ptr); return;
            case bool: PacketUtils.WriteInt(buffer, (bool)value ? 1 : 0, ref ptr); return;
            case ItemModel: (value as ItemModel)?.Write(buffer, ref ptr); return;
            case ItemModel[]: {
                    var arr = (ItemModel[])value;
                    PacketUtils.WriteUShort(buffer, (ushort)arr.Length, ref ptr);
                    for (int i = 0; i < arr.Length; i++)
                        arr[i].Write(buffer, ref ptr);
                }
                return;
            case PotionModel: (value as PotionModel)?.Write(buffer, ref ptr); return;
            case PotionModel[]: {
                    var arr = (PotionModel[])value;
                    PacketUtils.WriteUShort(buffer, (ushort)arr.Length, ref ptr);
                    for (int i = 0; i < arr.Length; i++)
                        arr[i].Write(buffer, ref ptr);
                }
                return;
        }
    }
}
