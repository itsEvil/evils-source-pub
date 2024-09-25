using GameServer.Core.Options;
using GameServer.Net;
using Shared;
using Shared.GameData;
using Shared.Redis;
using Shared.TimedSystems;
using System.Diagnostics;
using Resources = Shared.GameData.Resources;

namespace GameServer.Core;
public class Application : IDisposable {
    public static Application Instance { get; private set; }

    private bool _terminate = false;
    public AppOptions Options {get; private set;}
    public NetHandler NetHandler {get; private set;}
    public RedisDb Redis { get; private set; }
    public Resources Resources { get; private set; }
    public WorldsManager WorldManager { get; private set; }
    public Systems Systems { get; private set; }
    public int ExitCode = 0;
    private Application() {
        if (Instance != null)
            throw new Exception("Created another instance of Singleton::Application");
        Instance = this;
        NetHandler = new();
        Redis = RedisDb.Create();
        WorldManager = new();
        Systems = new();
    }
    public static Application Get() {
        return Instance ?? new Application();
    }
    public void Awake(AppOptions options) {
        Options = options;

        Resources = new Resources(options.Resources.GameDataPath);
        NetHandler.Init(options);
        Redis.Init(options.Redis.Host, options.Redis.Port, options.Redis.SyncTimeout, options.Redis.Index, options.Redis.Password);
        Redis.SetMe(options.Name, options.Address, options.Port, 0, options.MaxPopulation, 1);


        WorldManager.Init(options);

        Systems.Add<RedisUpdater>();

        Task.Run(NetHandler.AcceptConnections);
    }
    public int Run() {
        Task.Run(NetHandler.NetworkTick);

        Stopwatch watch = Stopwatch.StartNew();

        while (!_terminate) {
            watch.Restart();
            NetHandler.Tick();
            WorldManager.Tick();
            Systems.Update();
            

            watch.Stop();

            var sleep = Math.Abs(watch.ElapsedMilliseconds - Time.PerGameTick);
            if (sleep > 0)
                Thread.Sleep((int)sleep);
        }

        return ExitCode;
    }
    public void Stop() {
        _terminate = true;
    }
    public void Tick() {

    }
    public void Dispose() {

    }
}
