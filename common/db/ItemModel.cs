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
public class ItemModel(int id, string name, int[] stats, bool tradeable) {
    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public int[] Stats { get; set; } = stats;
    public bool Tradeable { get; set; } = tradeable;
}

public static class ItemModelExt {
    public static string Stringify(this ItemModel item) {
        return JsonSerializer.Serialize(item, SourceGenerationContext.Default.ItemModel);
    }
    public static string ToJson(this ItemModel[] array) {
        return JsonSerializer.Serialize(array, SourceGenerationContext.Default.ItemModelArray);
    }
}