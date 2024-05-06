using common;
using common.db;
using game_server.Networking;
using game_server.Core;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace game_server;
public sealed partial class CoreManager() {
    public static readonly RedisDb Redis = new();
    public static readonly Stopwatch Timer = Stopwatch.StartNew(); //Time since application start
    public ConcurrentDictionary<int, Client> Clients = [];
    public static bool Terminate = false;
    public void Run() {
        Resources.InitXmls("Debug");

        SLog.Info("CoreManager::Run");
        Task.Factory.StartNew(AcceptConnections);
        Task.Factory.StartNew(NetworkTick);


        GetWorld(World.NexusId, Resources.NameToWorldDesc["Nexus"]);
        RunGameLoop();
        SLog.Info("CoreManager::Run::End");
    }
}
