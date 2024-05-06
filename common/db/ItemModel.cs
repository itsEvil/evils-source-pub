using common.utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace common.db;

[method:JsonConstructor]
public class ItemModel(int itemType, string name, int[] stats, bool tradeable) {
    public static ItemModel Empty = new(-1, "", [], false);
    public int ItemType { get; set; } = itemType;
    public bool Tradeable { get; set; } = tradeable;
    public string Name { get; set; } = name;
    public int[] Stats { get; set; } = stats;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteInt(buffer, ItemType, ref ptr);
        PacketUtils.WriteBool(buffer, Tradeable, ref ptr);
        PacketUtils.WriteString(buffer, Name, ref ptr);
        PacketUtils.WriteByte(buffer, (byte)Stats.Length, ref ptr);
        for(int i = 0; i < Stats.Length; i++) {
            PacketUtils.WriteInt(buffer, Stats[i], ref ptr);
        }
    }
}

public static class ItemModelExt {
    public static string Stringify(this ItemModel item) {
        return JsonSerializer.Serialize(item, SourceGenerationContext.Default.ItemModel);
    }
    public static string ToJson(this ItemModel[] array) {
        return JsonSerializer.Serialize(array, SourceGenerationContext.Default.ItemModelArray);
    }
}