using System.Text.Json;
using System.Text.Json.Serialization;

namespace common.db;

[method: JsonConstructor]
public class PotionModel(int type, int count) {
    public int Type { get; set; } = type;
    public int Count { get; set; } = count;
}

public static class PotionModelExt {
    public static string Stringify(this PotionModel item) {
        return JsonSerializer.Serialize(item, SourceGenerationContext.Default.PotionModel);
    }
    public static string ToJson(this PotionModel[] array) {
        return JsonSerializer.Serialize(array, SourceGenerationContext.Default.PotionModelArray);
    }
}