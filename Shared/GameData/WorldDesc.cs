using System.Xml.Linq;

namespace Shared.GameData;
public sealed class WorldDesc {
    public static readonly WorldDesc Empty = new();


    public readonly uint UniqueId;
    public readonly string Name = "Empty";
    public readonly string Description = "";
    public readonly uint Difficulty;
    public readonly string FilePath = "";
    public readonly bool DisplayNames;
    public readonly byte ChunkSizeWidth;
    public readonly byte ChunkSizeHeight;
    public readonly uint DefaultTile;

    public readonly bool Template;
    public readonly bool IsEmpty;
    
    public WorldDesc() { IsEmpty = true; }
    public WorldDesc(XElement e, uint id, string name, string resourcePath) {
        UniqueId = id;
        Name = name;
        Description = e.ParseString("Desc", "");
        Difficulty = e.ParseUInt("Difficulty");
        ChunkSizeWidth = (byte)e.ParseUInt("ChunkWidth", undefined: 8);
        ChunkSizeHeight = (byte)e.ParseUInt("ChunkHeight", undefined: 8);
        DisplayNames = e.ParseBool("DisplayNames");
        Template = e.ParseBool("Template");
        FilePath = Path.Combine(resourcePath, e.ParseString("Path", ""));
        DefaultTile = e.ParseUInt("DefaultTile", undefined: 0);
    }
    public bool TryGetMapData(out byte[] data) {
        data = [];
        if(!File.Exists(FilePath)) {
            SLog.Error("Failed to find map data at {0} for {1}", args: [FilePath, Name]);
            return false;
        }

        try {
            data = File.ReadAllBytes(FilePath);
        }
        catch(Exception e)
        {
            SLog.Error("Caught exception {0} {1}", args: [e.Message, e.StackTrace]);
            return false;
        }
        return true;
    } 
}
