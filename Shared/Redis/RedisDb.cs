using Shared;
using Shared.GameData;
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
    public Character CreateCharacter(Account account, PlayerDesc desc) {        
        var newId = account.NextCharId++;
        var character = new Character(account, newId) {
            Stats = [.. desc.StatValues],
            MaxStats = [.. desc.StatMaxValues],
            Inventory = [.. desc.Inventory],
            Level = desc.Level,
            Exp = desc.Exp,
            ExpGoal = desc.ExpGoal,
        };

        character.FlushAsync();
        return character;
    }
    public Character[] GetAliveCharacters(Account account) {
        if (account.Alive.Length == 0)
            return [];

        Character[] chars = new Character[account.Alive.Length];
        for(var i = 0; i < account.Alive.Length; i++)
            chars[i] = new Character(account, account.Alive[i]);
        
        return chars;
    }

    private News[] CachedNews;
    private DateTime LastNewsTime;
    public News[] GetNews() {
        if (Globals == null)
            LoadGlobals();

        News[] news = null;
        //Reload news
        if(DateTime.Now - LastNewsTime > TimeSpan.FromMinutes(5)) {
            List<News> current = [];
            for(int i = 0; i < Globals.News.Length; i++) {
                var @new = new News(Database, Globals.News[i]);
                if(DateTime.Now - @new.End > TimeSpan.Zero) {
                    Database.KeyDelete(@new.Key);
                    continue;
                }

                current.Add(@new);
            }

            news = [.. current];

            Globals.News = new uint[news.Length];
            for(int i = 0; i < news.Length; i++) {
                Globals.News[i] = news[i].Id;
            }

            Globals.FlushAsync();
            CachedNews = news;
            LastNewsTime = DateTime.Now;
        } 
        else
        {
            news = CachedNews;
        }


        return news;
    }
}
