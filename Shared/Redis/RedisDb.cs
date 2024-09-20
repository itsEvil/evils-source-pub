using Shared;
using Shared.Redis.Models;
using StackExchange.Redis;
using System.Security.Principal;

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

        if(!Database.KeyExists("globals")) {
            Globals.NextAccountId = 0;
            Globals.NextGuildId = 0;
            Globals.NextItemId = 0;
        }

        Globals.FlushAsync();
        SLog.Info("Loaded globals: {0}", args: [Globals.ToString()]);
    }

    public bool TryGetAccount(string email, out Account account) {
        account = null;
        if(!Login.TryGetLogin(Database, email, out var login))
            return false;

        account = new Account(Database, login.AccountId);
        if(account.IsNull) 
            return false;
        
        return true;
    }

    public bool TryLogin(string email, string password) {
        if (!Login.TryGetLogin(Database, email, out var login))
            return false;

        var hashed = Convert.ToBase64String((password + login.Salt).ToSHA512());

        if (hashed != login.HashedPassword)
            return false;

        return true;
    }
    //This method assumes you already checked if a account exists with the same email
    public Account CreateAccount(string email, string password) {
        var newId = Globals.NextAccountId++;
        var login = Login.Create(Database, email, password, newId);
        var account = new Account(Database, login.AccountId) { 
            Rank = Rank.None,
        };

        account.FlushAsync();

        return account;
    }
}
