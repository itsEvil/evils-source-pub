using GameServer.Core.Options;
using GameServer.Game.Worlds;
using Shared;
using Shared.GameData;
using System.Runtime.InteropServices;

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
            /* 
            var mapData = Map.WriteMapToBuffer(new Map(
#if DEBUG
                desc,
#endif       
                128, 128, 8, 8, 0));
            
            File.WriteAllBytes(desc.FilePath, mapData.ToArray());
            */

            if(!desc.TryGetMapData(out var data))
            {
                SLog.Error("Failed to get map data for {0} \n\t at {1}", args: [desc.Name, desc.FilePath]);
                return null;
            }

            var r = new Reader();
            map = new Map(
#if DEBUG
                desc,
#endif       
                r, data);

            SLog.Debug("Converted map from disk with {0} chunks and {1} region lists", args: [map.Chunks.Length, map.Regions.Count]);

            m_Converted[desc] = map;
        }

        World world;
        if (desc.Template) {
            world = new World(GetNextWorldId(), desc);
            Worlds[world.Id] = world;
            world.Init(m_Converted[desc]);
            return world;
        }

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

}
public class WorldIds(uint townId)
{
    public readonly uint TownId = townId;
    public void Write(Writer w, Span<byte> b)
    {
        w.Write(b, (byte)0);
        w.Write(b, TownId);
    }
}
