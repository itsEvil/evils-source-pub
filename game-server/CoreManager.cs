using common;
using common.db;
using game_server.Networking;
using game_server.Core;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace game_server;
public sealed partial class CoreManager {
    public static readonly RedisDb Redis = new();
    public static readonly Stopwatch Timer = Stopwatch.StartNew(); //Time since application start
    public ConcurrentDictionary<int, Client> Clients = [];
    public static bool Terminate = false;
    public CoreManager() {
        SLog.Info("CoreManager::ctor");
        Resources.InitXmls("Debug");
        _ = Task.Factory.StartNew(AcceptConnections);
        _ = Task.Factory.StartNew(NetworkTick);
        
        GetWorld(World.NexusId, Resources.NameToWorldDesc["Nexus"]);
    }
    public async Task<int> Run() {
#if DEBUG
        try {
#endif
            var watch = Stopwatch.StartNew();
            watch.Restart();
            Transaction = Redis.Database.CreateTransaction();
            //SLog.Debug("Worlds::{0}::Tick::{1}::ToAdd::{2}::ToRemove::{3}", Worlds.Count, Time.GameTickCount, _toAddWorlds.Count, _toRemoveWorlds.Count);
            for (int i = 0; i < _toRemoveWorlds.Count; i++)
            {
                var id = _toRemoveWorlds[i];
                Worlds.Remove(id);
            }
            _toRemoveWorlds.Clear();

            for (int i = 0; i < _toAddWorlds.Count; i++)
            {
                var world = _toAddWorlds[i];
                if (Worlds.ContainsKey(world.Id))
                {
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
            Time.GameTickCount++;
            return sleepTime;
#if DEBUG
        } catch(Exception e) {
            SLog.Fatal(e);
        }
#endif
        return Time.PerGameTick;
    }
}
