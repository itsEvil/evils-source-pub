using Shared.Redis.Models;
using System.Xml.Linq;

namespace Shared.GameData;
public sealed class Resources {
    private static uint m_UniqueId = 0;

    public readonly Dictionary<uint, TileDesc> Id2Tile = [];
    public readonly Dictionary<string, TileDesc> Name2Tile = [];

    /// <summary>
    /// Only used by XML objects that are not set to a certain type.
    /// </summary>
    public readonly Dictionary<uint, ObjectDesc> Id2Object = [];
    /// <summary>
    /// Contains names of every XML object regardless of type.
    /// </summary>
    public readonly Dictionary<string, ObjectDesc> Name2Object = [];

    public readonly Dictionary<uint, PlayerDesc> Id2Player = [];
    public readonly Dictionary<string, PlayerDesc> Name2Class = [];

    public readonly Dictionary<uint, ProjectileDesc> Id2Projectile = [];
    public readonly Dictionary<string, ProjectileDesc> Name2Projectile = [];

    public readonly Dictionary<uint, ItemDesc> Id2Item = [];
    public readonly Dictionary<string, ItemDesc> Name2Item = [];

    public readonly Dictionary<uint, CharacterDesc> Id2Enemy = [];
    public readonly Dictionary<string, CharacterDesc> Name2Enemy = [];



    public readonly Dictionary<string, WorldDesc> Name2Worlds = [];
    public readonly Dictionary<uint, WorldDesc> Id2Worlds = [];

    public Resources(string resourcePath = "") {
        if (string.IsNullOrEmpty(resourcePath))
            throw new Exception("Resource path is null or empty");

        ParseFiles(Path.Combine(resourcePath, "Xmls"));
        ParseWorldFiles(Path.Combine(resourcePath, "Worlds"));
    }
    private void ParseFiles(string path) {
        var files = Directory.GetFiles(path, "*.xml");
        SLog.Debug("Game data location: {0}", args: [path]);
        foreach (var file in files) {
            SLog.Debug("Parsing: {0}", args: [file]);

            try
            {
                var text = File.ReadAllText(file);

                var xml = XElement.Parse(text);

                ParseFile(xml);
            }
            catch(Exception e)
            {
                SLog.Error("Error parsing: '{0}' {1} {2}", args: [file, e.Message, e.StackTrace]);
            }

        }

        SLog.Debug("Loaded game data!");
        SLog.Debug("Tiles: {0}", args: [Name2Tile.Count]);
        SLog.Debug("Objects: {0}", args: [Name2Object.Count]);
        SLog.Debug("Classes: {0}", args: [Name2Class.Count]);
        SLog.Debug("Projectiles: {0}", args: [Name2Projectile.Count]);
        SLog.Debug("Items: {0}", args: [Name2Item.Count]);
    }

    private void ParseWorldFiles(string path)
    {
        var files = Directory.GetFiles(path, "*.xml");
        foreach (var file in files)
        {
            SLog.Debug("Parsing: {0}", args: [file]);

            try
            {
                var text = File.ReadAllText(file);

                var xml = XElement.Parse(text);

                ParseWorlds(xml, path);
            }
            catch (Exception e)
            {
                SLog.Error("Error parsing: '{0}' {1} {2}", args: [file, e.Message, e.StackTrace]);
            }

        }
    }

    private void ParseFile(XElement file) {
        foreach (var tile in file.Elements("Tile"))
        {
            var name = tile.ParseString("@name", "unknown");
            var id = tile.ParseUInt("@id", false, 0);
            var desc = Name2Tile[name] = Id2Tile[id] = new TileDesc(tile, id, name);
            Id2Object[desc.UniqueId] = desc;
        }

        foreach (var e in file.Elements("Class"))
        {
            var name = e.ParseString("@name", "unknown");
            var id = e.ParseUInt("@id", false, 0);
            var desc = Name2Object[name] = Id2Player[id] = Name2Class[name] = new PlayerDesc(e, id, name);
            Id2Object[desc.UniqueId] = desc;
        }

        foreach (var e in file.Elements("Character"))
        {
            var name = e.ParseString("@name", "unknown");
            var id = e.ParseUInt("@id", false, 0);
            var desc = Name2Object[name] = Id2Player[id] = Name2Class[name] = new PlayerDesc(e, id, name);
            Id2Object[desc.UniqueId] = desc;
        }

        foreach (var e in file.Elements("Item"))
        {
            var name = e.ParseString("@name", "unknown");
            var id = e.ParseUInt("@id", false, 0);
            var desc = Name2Object[name] = Id2Item[id] = Name2Item[name] = new ItemDesc(e, id, name);
            Id2Object[desc.UniqueId] = desc;
        }

        foreach (var e in file.Elements("Projectile"))
        {
            var name = e.ParseString("@name", "unknown");
            var id = e.ParseUInt("@id", false, 0);
            var desc = Name2Object[name] = Id2Projectile[id] = Name2Projectile[name] = new ProjectileDesc(e, id, name);
            Id2Object[desc.UniqueId] = desc;
        }
    }

    private void ParseWorlds(XElement file, string path)
    {
        foreach (var world in file.Elements("World"))
        {
            var name = world.ParseString("@name", "unknown");
            var id = world.ParseUInt("@id", false, 0);
            WorldDesc desc = Name2Worlds[name] = Id2Worlds[id] = new WorldDesc(world, id, name, path);

            SLog.Debug("Parsed: {0} {1} world \n\t at {2}", args: [name, id, desc.FilePath]);
            //Load map data
        }
    }

    public static uint GetNextUniqueId()
    {
        return m_UniqueId++;
    }
}
