using Shared;
using Shared.Redis.Models;
using StackExchange.Redis;

namespace Shared.Redis;
public sealed class RedisDb {
    public ConnectionMultiplexer Redis;
    public IServer Server;
    public IDatabase Database;
    public ISubscriber Sub;

    public Global Globals;

    public void Init(string host, int port, int syncTimeout, int index, string password) {
        var conString = host + ":" + port + ",syncTimeout=" + syncTimeout;
        if (!string.IsNullOrWhiteSpace(password))
            conString += ",password=" + password;

        Redis = ConnectionMultiplexer.Connect(conString);
        Server = Redis.GetServer(Redis.GetEndPoints(true)[0]);
        Database = Redis.GetDatabase(index);
        Sub = Redis.GetSubscriber();

        SLog.Info("Connected to redis at [{0}:{1}]", args: [host, port]);
        LoadGlobals();
    }
    public void LoadGlobals() {
        Globals = new Global(Database);
        SLog.Info("Loaded globals: {0}", args: [Globals.ToString()]);
    }
}
