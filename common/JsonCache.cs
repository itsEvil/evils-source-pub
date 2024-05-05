using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using common.db;
using Microsoft.Win32;

namespace common;
public static class JsonCache {
    public static JsonSerializerOptions Options = new() {
        TypeInfoResolver = SourceGenerationContext.Default,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement,
        PropertyNameCaseInsensitive = true,
    };
}

[JsonSerializable(typeof(ItemModel))]
[JsonSerializable(typeof(ItemModel[]))]
[JsonSerializable(typeof(PotionModel[]))]
[JsonSerializable(typeof(PotionModel))]
[JsonSerializable(typeof(DbLoginInfo))]
[JsonSerializable(typeof(string[]))]
public partial class SourceGenerationContext : JsonSerializerContext
{

}

public static class StringArrayExtensions {
    public static string ToJson(this string[] array) {
        return JsonSerializer.Serialize(array, SourceGenerationContext.Default.StringArray);
    }
}