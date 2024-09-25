using StackExchange.Redis;

namespace Shared.Redis.Models;
public static class Names {
    public static bool Add(IDatabase db, string name, uint id) {
        db.HashSet("names", name, id);
        return true;
    }
    public static bool Exists(IDatabase db, string name)
        => db.HashExists("names", name);
    public static uint GetId(IDatabase db, string name) {
        var ret = db.HashGet("names", name);
        if (ret == RedisValue.Null)
            return 0;

        return (uint)ret;
    }
    public static Account GetAccount(IDatabase db, string name) {
        var id = GetId(db, name);
        if (id == 0)
            return null;

        return new Account(db, id);
    }
}
