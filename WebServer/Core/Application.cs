using Shared;
using Shared.GameData;
using Shared.Redis;
using Shared.TimedSystems;
using System.Diagnostics;
using WebServer.Core.Options;
using Resources = Shared.GameData.Resources;

namespace WebServer.Core;
public class Application : IDisposable {
    public static Application Instance { get; private set; }

    private bool _terminate = false;
    public AppOptions Options {get; private set;}
    public NetHandler NetHandler {get; private set;}
    public RedisDb Redis { get; private set; }
    public Resources Resources { get; private set; }
    public Systems Systems { get; private set; }
    private Application() {
        if (Instance != null)
            throw new Exception("Created another instance of Singleton::Application");
        Instance = this;
        NetHandler = new();
        Redis = RedisDb.Create();
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
        Redis.SetMe(options.Name, options.Address, options.Port, 0, options.MaxPopulation, 0);
        Systems.Add<RedisUpdater>();


        Task.Run(NetHandler.AcceptConnections);
    }
    public int Run() {
        Task.Run(NetHandler.NetworkTick);

        var watch = Stopwatch.StartNew();

        while (!_terminate) {
            watch.Restart();
            NetHandler.Tick();
            Systems.Update();

            watch.Stop();

            var sleep = Math.Abs(watch.ElapsedMilliseconds - Time.PerGameTick);
            if (sleep > 0)
                Thread.Sleep((int)sleep);
        }

        return 0;
    }
    public void Stop() {
        _terminate = true;
    }
    public void Dispose() {
        Systems.Dispose();
    }
}
