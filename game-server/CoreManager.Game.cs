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

namespace game_server;
public sealed partial class CoreManager {
    public ITransaction Transaction;
    public Dictionary<int, World> Worlds = []; //worlds in the server
    private readonly List<World> _toAddWorlds  = [];
    private readonly List<int> _toRemoveWorlds = [];

    private List<Task> _gameTasks = new(MaxConnections);
    public async void RunGameLoop() {
        var watch = Stopwatch.StartNew();
        
        while (!Terminate) {
            watch.Restart();
            Transaction = Redis.Database.CreateTransaction();
            //SLog.Debug("Worlds::{0}::Tick::{1}::ToAdd::{2}::ToRemove::{3}", Worlds.Count, Time.GameTickCount, _toAddWorlds.Count, _toRemoveWorlds.Count);
            for (int i = 0; i < _toRemoveWorlds.Count; i++) {
                var id = _toRemoveWorlds[i];
                Worlds.Remove(id);
            }
            _toRemoveWorlds.Clear();

            for (int i = 0; i < _toAddWorlds.Count; i++) {
                var world = _toAddWorlds[i];
                if (Worlds.ContainsKey(world.Id)) {
                    SLog.Error("WorldAlreadyExistsWithId::{0}::Overwriting!", world.Id);
                }

                Worlds[world.Id] = world;
            }
            _toAddWorlds.Clear();

            foreach (var (_, client) in Clients)
                _gameTasks.Add(client.Tick());

            await Task.WhenAll(_gameTasks);
            _gameTasks.Clear();

            foreach (var (_, world) in Worlds)
                _gameTasks.Add(world.Tick());

            await Task.WhenAll(_gameTasks);
            _gameTasks.Clear();

            await Transaction.ExecuteAsync();
            var elapsed = watch.Elapsed.TotalMilliseconds;
            var sleepTime = Math.Max(0, Time.PerGameTick - (int)elapsed);
            Thread.Sleep(sleepTime);
            Time.GameTickCount++;
        }
    }
    public void AddWorld(World world) {
        _toAddWorlds.Add(world);
    }
    public void RemoveWorld(int worldId) {
        _toRemoveWorlds.Add(worldId);
    }
    public World GetWorld(int id, WorldDesc descriptor, Client client = null) {
        if(Worlds.TryGetValue(id, out var world)) {
            if (client is not null)
                world.IsAllowedAccess(client);

            return world;
        }

        world = World.Resolve(id);
        world.Init(descriptor, client);

        AddWorld(world);
        return world;
    }
}
