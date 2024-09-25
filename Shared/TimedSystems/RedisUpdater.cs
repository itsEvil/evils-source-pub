using Shared.Interfaces;
using Shared.Redis;
using Shared.Redis.Models;

namespace Shared.TimedSystems;
public sealed class RedisUpdater : ISystem {
    public int Cooldown { get; init; } = 15_000;
    public int Retries { get; set; } = -1;
    public long LastRunTime { get; set; }
    private static RedisDb Redis => RedisDb.Instance;

    private readonly List<int> ToRemove = [];
    private readonly List<bool> Found = [];

    public void Execute() {
        //SLog.Debug("Server list");
        //foreach(var server in Redis.Servers)
        //{
        //    SLog.Debug("Server: {0}", args: [server.Name]);
        //}

        //Update this servers info
        Redis.UpdateMe(RedisDb.Population);
        Servers.Update(Redis.Database, Redis.Me.Name, Redis.Me.ServerType);

        //Update other servers info
        var keys = Servers.GetServerKeys(Redis.Database);

        ToRemove.Clear();
        Found.Clear();
        
        foreach(var key in keys) {
            Found.Add(false);

            for(int i = 0; i < Redis.Servers.Count; i++) {
                var server = Redis.Servers[i];
                if (server.Name == key) {
                    Found[i] = true;
                    continue;
                }

                //Local Server list contains server name but database doesnt anymore
                //so we need to remove
                ToRemove.Add(i);
            }
        }

        foreach (var idx in ToRemove) {
            if (idx >= Redis.Servers.Count)
                continue;

            Redis.Servers.RemoveAt(idx);
        }

        for(int i = 0; i < keys.Length; i++) {
            if (Found[i])
                continue;

            var words = keys[i].Split('.');
            if(words.Length != 3) {
                SLog.Warn("Keys.Words length is not 3");
                continue;
            }

            if (!int.TryParse(words[1], out var type)) {
                SLog.Warn("ServerType failed to parse to int");
                continue;
            }

            Redis.Servers.Add(new Server(Redis.Database, words[^1], type));
        }

        Redis.ServersArray = [.. Redis.Servers];
    }
}
