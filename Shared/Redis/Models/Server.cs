using Shared.Interfaces;
using Shared.Redis;
using StackExchange.Redis;

namespace Shared.Redis.Models;
public sealed class Server : RedisObject, IWriteable
{
    public string Name { get => GetValue<string>("name"); set => SetValue("name", value); }
    public string IPAddress { get => GetValue<string>("address"); set => SetValue("address", value); }
    public int Port { get => GetValue<int>("port"); set => SetValue("port", value); }
    public int Population { get => GetValue<int>("pop"); set => SetValue("pop", value); }
    public int MaxPopulation { get => GetValue<int>("maxPop"); set => SetValue("maxPop", value); }
    public int ServerType { get => GetValue<int>("serverType"); set => SetValue("serverType", value); }
    //Type 0 == WebServer, 1 == GameServer
    public Server(IDatabase db, string name, int type = 0, string field = null, bool isAsync = false) : base(db, "server." + type + '.' + name, field, isAsync) { }
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, Name);
        w.Write(b, IPAddress);
        w.Write(b, Port);
        w.Write(b, Population);
        w.Write(b, MaxPopulation);
        w.Write(b, (ushort)ServerType);
    }
}
