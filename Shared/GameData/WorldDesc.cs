using System.Xml.Linq;

namespace Shared.GameData;
public sealed class WorldDesc {
    public readonly uint UniqueId;
    public readonly string Name;
    public readonly string Description;
    public readonly uint Difficulty;
    public readonly string FilePath;
    public readonly bool DisplayNames;
    public readonly byte ChunkSize;
    public WorldDesc(XElement e, uint id, string name, string resourcePath) {
        UniqueId = id;
        Name = name;
        Description = e.ParseString("Description", "");
        Difficulty = e.ParseUInt("Difficulty");
        ChunkSize = (byte)e.ParseUInt("ChunkSize", undefined: 8);
        DisplayNames = e.ParseBool("DisplayNames");
        FilePath = Path.Combine(resourcePath, e.ParseString("Path", ""));
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
