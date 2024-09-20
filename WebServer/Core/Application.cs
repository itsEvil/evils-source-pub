using Shared.Redis;
using WebServer.Core.Options;

namespace WebServer.Core;
public class Application : IDisposable {
    public static Application Instance { get; private set; }

    private bool _terminate = false;
    public AppOptions Options {get; private set;}
    public NetHandler NetHandler {get; private set;}
    public RedisDb Redis { get; private set; }
    private Application() {
        if (Instance != null)
            throw new Exception("Created another instance of Singleton::Application");
        Instance = this;
        NetHandler = new();
        Redis = new();
    }
    public static Application Get() {
        return Instance ?? new Application();
    }
    public void Awake(AppOptions options) {
        Options = options;

        NetHandler.Init(options);
        Redis.Init(options.Redis.Host, options.Redis.Port, options.Redis.SyncTimeout, options.Redis.Index, options.Redis.Password);

        Task.Run(NetHandler.AcceptConnections);
    }
    public int Run() {
        Task.Run(NetHandler.NetworkTick);
        
        while (!_terminate) {
            //var threads = Process.GetCurrentProcess().Threads;
            //SLog.Debug("Threads: {0}", args: [threads.Count]);
            NetHandler.Tick();

            Thread.Sleep(1);
            //move to seperate thread
        }

        return 0;
    }
    public void Stop() {
        _terminate = true;
    }
    public void Tick() {

    }
    public void Dispose() {

    }
}
