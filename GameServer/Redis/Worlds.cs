using GameServer.Game.Worlds;
using Shared;
using Shared.Redis.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameServer.Redis;
public static class Worlds {
    public enum WorldsAction {
        UpdateTimer,
        Remove,
    }
    public static void Add(IDatabase db, Server server, World world)
    {
        var sb = new StringBuilder();

        sb.Append("server.");
        sb.Append(server.ServerType);
        sb.Append('.');
        sb.Append(server.Name);
        sb.Append(".world.");
        sb.Append(world.Id);
        //var key = "world." + type + '.' + name;

        var key = sb.ToString();

        db.HashSet("worlds", key, World.ToRedis(world));
        db.HashFieldExpireAsync("worlds", [key], TimeSpan.FromMinutes(60));
    }
    public static void Update(IDatabase db, Server server, World world, WorldsAction action = WorldsAction.UpdateTimer)
    {
        var sb = new StringBuilder();

        sb.Append("server.");
        sb.Append(server.ServerType);
        sb.Append('.');
        sb.Append(server.Name);
        sb.Append(".world.");
        sb.Append(world.Id);

        var key = sb.ToString();

        switch (action) {
            case WorldsAction.UpdateTimer: {
                var results = db.HashFieldExpire("worlds", [key], TimeSpan.FromMinutes(1));
                if (results.Length == 0)
                {
                    SLog.Error("Key {0} does not exist", args: [key]);
                    return;
                }

#if DEBUG
                foreach (var result in results)
                {
                    SLog.Info("Update result {0}", args: [result]);
                }
#endif
                break;
            }
            case WorldsAction.Remove: {
                var results = db.HashFieldExpire("worlds", [key], TimeSpan.FromSeconds(5));
                if (results.Length == 0) {
                    SLog.Error("Key {0} does not exist", args: [key]);
                    return;
                }

                break;
            }
        }
    }
    public static string[] GetServerKeys(IDatabase db)
    {
        var values = db.HashGetAll("servers");
        if (values.Length == 0)
            return [];

        string[] ret = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
            ret[i] = values[i].Value;

        return ret;
    }
}


