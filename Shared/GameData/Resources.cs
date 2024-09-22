using System.Xml.Linq;

namespace Shared.GameData;
public sealed class Resources {
    public const uint EmptyObjId = 0;
    public const uint EmptyItemId = 1;
    public const uint EmptyPlayerId = 2;
    public const uint EmptyProjectileId = 3;
    public const uint EmptyEnemyId = 4;

    public readonly Dictionary<uint, ObjectDesc> Id2Object = [];
    public readonly Dictionary<string, ObjectDesc> Name2Object = [];
    
    public readonly Dictionary<uint, PlayerDesc> Id2Player = [];
    public readonly Dictionary<string, PlayerDesc> Name2Player = [];

    public readonly Dictionary<uint, ProjectileDesc> Id2Projectile = [];
    public readonly Dictionary<string, ProjectileDesc> Name2Projectile = [];

    public readonly Dictionary<uint, ItemDesc> Id2Item = [];
    public readonly Dictionary<string, ItemDesc> Name2Item = [];

    public readonly Dictionary<uint, EnemyDesc> Id2Enemy = [];
    public readonly Dictionary<string, EnemyDesc> Name2Enemy = [];

    public Resources(string resourcePath = "") {
        if (string.IsNullOrEmpty(resourcePath))
            throw new Exception("Resource path is null or empty");

        ParseFiles(resourcePath);
    }
    private void ParseFiles(string path) {
        var files = Directory.GetFiles(path, "*.xml");
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
    }
    private void ParseFile(XElement file) {
        foreach(var obj in file.Elements("Object")) {
            var className = obj.ParseString("Class");
            var @class = obj.ParseEnum("Class", ClassType.GameObject);
            var name = obj.ParseString("@name", "unknown");
            var id = obj.ParseUInt("@id", false, 0);

            var desc = @class switch {
                ClassType.GameObject => Name2Object[name] = Id2Object[id] = new ObjectDesc(obj, id, name),
                ClassType.Item => Name2Object[name] = Id2Object[id] = Id2Item[id] = Name2Item[name] = new ItemDesc(obj, id, name),
                ClassType.Projectile => Name2Object[name] = Id2Object[id] = Id2Projectile[id] = Name2Projectile[name] = new ProjectileDesc(obj, id, name),
                ClassType.Enemy => Name2Object[name] = Id2Object[id] = Id2Enemy[id] = Name2Enemy[name] = new EnemyDesc(obj, id, name),
                ClassType.Player => Name2Object[name] = Id2Object[id] = Id2Player[id] = Name2Player[name] = new PlayerDesc(obj, id, name),
                _ => throw new Exception($"XML Object Class '{(className == null ? "null" : className)}' not recognised as a valid value.")
            };


            SLog.Debug("Parsed: {0} {1}", args: [desc.Name, desc.Id]);
        }
    }
}
