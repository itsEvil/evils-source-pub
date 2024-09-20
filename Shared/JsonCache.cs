using System.Text.Json.Serialization;
using System.Text.Json;
using Shared.Redis;
using Shared.Redis.Models;

namespace Shared;
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

[JsonSerializable(typeof(ItemData))]
[JsonSerializable(typeof(ItemData[]))]
[JsonSerializable(typeof(string[]))]
public partial class SourceGenerationContext : JsonSerializerContext
{

}

public static class StringArrayExtensions {
    public static string ToJson(this string[] array) {
        return JsonSerializer.Serialize(array, SourceGenerationContext.Default.StringArray);
    }
}