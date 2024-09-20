using Shared.Redis;
using WebServer.Core.Options;

namespace WebServer.Core;
public class Application : IDisposable {
    private bool _terminate = false;
    private AppOptions _options;
    private NetHandler _handler;
    private RedisDb _redis;
    public Application()
    {
        _handler = new();
        _redis = new();
    }
    public void Awake(AppOptions options) {
        _options = options;

        _handler.Init(options);
        _redis.Init(options.Redis.Host, options.Redis.Port, options.Redis.SyncTimeout, options.Redis.Index, options.Redis.Password);

        _redis.Globals.NextAccountId += 5;
        _redis.Globals.FlushAsync();

        Task.Run(_handler.AcceptConnections);
    }
    public int Run() {
        Task.Run(_handler.NetworkTick);
        
        while (!_terminate) {
            //var threads = Process.GetCurrentProcess().Threads;
            //SLog.Debug("Threads: {0}", args: [threads.Count]);
            _handler.Tick();

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
