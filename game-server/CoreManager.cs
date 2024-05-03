using common;
using game_server.Networking;
using game_server.Realm;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace game_server;
public sealed partial class CoreManager() {
    public static readonly Stopwatch Timer = Stopwatch.StartNew(); //Time since application start
    public ConcurrentDictionary<int, Client> Clients = [];
    public static bool Terminate = false;
    public void Run() {
        SLog.Info("CoreManager::Run");
        Task.Factory.StartNew(AcceptConnections);
        Task.Factory.StartNew(NetworkTick);
        RunGameLoop();
        SLog.Info("CoreManager::Run::End");
    }
}
