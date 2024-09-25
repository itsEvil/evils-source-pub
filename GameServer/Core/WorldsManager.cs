using GameServer.Core.Options;
using GameServer.Game.Worlds;
using Shared.GameData;

namespace GameServer.Core;
public sealed class WorldsManager {
    public readonly Dictionary<uint, World> Worlds = [];
    private readonly List<Task> WorldTasks = new(256);
    public void Init(AppOptions options) {
        
    }

    public World Create(WorldDesc desc)
    {
        return null;
    }

    public World Get(uint id)
    {
        return null;
    }

    public async void Tick() {
        foreach (var (_, world) in Worlds)
            WorldTasks.Add(world.Tick());

        await Task.WhenAll(WorldTasks);
        WorldTasks.Clear();
    }
}
