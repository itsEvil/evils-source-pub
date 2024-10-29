using GameServer.Core.Options;
using GameServer.Game.Worlds;
using Shared;
using Shared.GameData;

namespace GameServer.Core;
public sealed class WorldsManager {
    private uint NextWorldId = uint.MaxValue;
    public readonly Dictionary<uint, World> Worlds = [];
    private readonly Dictionary<WorldDesc, Map> m_Converted = [];
    private readonly List<Task> WorldTasks = new(256);
    private static Resources Resources => Application.Instance.Resources;
    public void Init(AppOptions options) {
        Create(Resources.Name2Worlds["Town"]);
    }

    public World Create(WorldDesc desc)
    {
        if(!m_Converted.TryGetValue(desc, out var map))
        {
            //Write a basic map for testing
            //var mapData = CreateMapData(new Map(128, 128, 8, 8, 0, false));
            //File.WriteAllBytes(desc.FilePath, mapData.ToArray());


            if(!desc.TryGetMapData(out var data))
            {
                SLog.Error("Failed to get map data for {0} \n\t at {1}", args: [desc.Name, desc.FilePath]);
                return null;
            }

            map = ConvertDataToMap(data);

            m_Converted[desc] = map;
        }

        World world;
        if (desc.Template)
            world = new World(GetNextWorldId(), desc);

        if (!Worlds.TryGetValue(desc.UniqueId, out world))
        {
            world = new World(desc.UniqueId, desc);
            Worlds[desc.UniqueId] = world;
        }

        world.Init(m_Converted[desc]);

        SLog.Debug("Created world {0}-{1} \n\t from {2}", args: [world.Desc.Name, world.Id, desc.FilePath]);

        return world;
    }

    public World Get(uint id) {
        if(Worlds.TryGetValue(id, out var world))
            return world;

        return null;
    }

    public async void Tick() {
        foreach (var (_, world) in Worlds)
            WorldTasks.Add(world.Tick());

        await Task.WhenAll(WorldTasks);
        WorldTasks.Clear();
    }
    private uint GetNextWorldId() {
        NextWorldId -= 1;
        return NextWorldId;
    }
    private static Map ConvertDataToMap(byte[] mapData) {
        var r = new Reader();
        r.Reset(mapData.Length);
        var b = mapData.AsSpan();

        var width = r.UInt(b);
        var height = r.UInt(b);

        var chunkWidth = r.Byte(b);
        var chunkHeight = r.Byte(b);
        
        var initValue = r.UInt(b);

        var map = new Map(width, height, chunkWidth, chunkHeight, initValue, true);

        return map;
    }
    private static Span<byte> CreateMapData(Map map) {
        var w = new Writer();
        w.Reset();
        var array = new byte[512];
        var b = array.AsSpan();

        w.Write(b, map.Width);
        w.Write(b, map.Height);
        w.Write(b, map.ChunkSizeWidth);
        w.Write(b, map.ChunkSizeHeight);
        w.Write(b, map.InitValue);

        return b[0..w.Position];
    }
}
