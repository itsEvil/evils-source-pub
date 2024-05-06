using common.utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace common.db;

[method: JsonConstructor]
public class PotionModel(int itemType, int count) {
    public readonly static PotionModel Empty = new(-1, 0);
    public int ItemType { get; set; } = itemType;
    public int Count { get; set; } = count;
    public void Write(Span<byte> buffer, ref int ptr) {
        PacketUtils.WriteInt(buffer, ItemType, ref ptr);
        PacketUtils.WriteInt(buffer, Count, ref ptr);
    }
}

public static class PotionModelExt {
    public static string Stringify(this PotionModel item) {
        return JsonSerializer.Serialize(item, SourceGenerationContext.Default.PotionModel);
    }
    public static string ToJson(this PotionModel[] array) {
        return JsonSerializer.Serialize(array, SourceGenerationContext.Default.PotionModelArray);
    }
}