using common;
using game_server.Networking;
using game_server.Core;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game_server.Core.Logic;

namespace game_server;
public sealed partial class CoreManager {
    public ITransaction? Transaction = null;
    public Dictionary<int, World> Worlds = []; //worlds in the server
    private readonly List<World> _toAddWorlds  = [];
    private readonly List<int> _toRemoveWorlds = [];
    private readonly BehaviorDb BehaviourDb;

    private List<Task> _gameTasks = new(MaxConnections);
    public void AddWorld(World world) {
        _toAddWorlds.Add(world);
    }
    public void RemoveWorld(int worldId) {
        _toRemoveWorlds.Add(worldId);
    }
    public World GetWorld(int id, WorldDesc descriptor, Client? client = null) {
        if(Worlds.TryGetValue(id, out var world)) {
            if (client is not null)
                world.IsAllowedAccess(client);

            return world;
        }

        world = World.Resolve(id, this);
        world.Init(descriptor, client);

        AddWorld(world);
        return world;
    }
}
