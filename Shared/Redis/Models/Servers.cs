using StackExchange.Redis;

namespace Shared.Redis.Models;
public sealed class Servers {
    public static void Add(IDatabase db, string name, int type) {
        var key = "server." + type + '.' + name;

        db.HashSet("servers", key, key);
        db.HashFieldExpireAsync("servers", [key], TimeSpan.FromMinutes(1));
    }
    public static void Update(IDatabase db, string name, int type) {
        var key = "server." + type + '.' + name;
        var results = db.HashFieldExpire("servers", [key], TimeSpan.FromMinutes(1));
        if(results.Length == 0) {
            SLog.Error("Key {0} does not exist", args: [key]);
            return;
        }

        foreach(var result in results)
        {
            SLog.Info("Update result {0}", args: [result]);
        }
    }
    public static string[] GetServerKeys(IDatabase db) {
        var values = db.HashGetAll("servers");
        if (values.Length == 0)
            return [];
        
        string[] ret = new string[values.Length];
        for(int i = 0; i < values.Length; i++)
            ret[i] = values[i].Value;
        
        return ret;
    }
}
