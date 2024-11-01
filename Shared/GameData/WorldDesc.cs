using System.Xml.Linq;

namespace Shared.GameData;

public enum WorldType {
    World,
    Submerged,
    Arena,
}
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

    //Submerged world
    public readonly DrainInfo Drain = DrainInfo.Empty;

    public readonly bool Template;
    public readonly bool IsEmpty;
    
    public readonly WorldType Type;
    private WorldDesc() { IsEmpty = true; }
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
        Type = e.ParseEnum("Type", WorldType.World);

        var drain = e.Element("Drain");

        if(drain != null) {
            Drain = new DrainInfo(drain);
        }
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
    public override string ToString()
    {
        return $"{Name} \n\t at {FilePath}";
    }
    
    //Submerged world
    public class DrainInfo {
        public static readonly DrainInfo Empty = new DrainInfo();
        public readonly int Frequency = 0; //MS between each drain, 0 == each tick, 1_000 == once per second
        public readonly int Amount = 0; //Amount per frequency to drain
        public readonly int ResourceType = 0; //0 == HP, 1 == Mana
        private DrainInfo() { }
        public DrainInfo(XElement e) {
            Frequency = e.ParseInt("Frequency", 1_000);
            Amount = e.ParseInt("Amount", 25);
            ResourceType = e.ParseInt("ResourceType", 0);
        }
    }
}
