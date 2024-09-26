using Shared.GameData;
using Shared.Redis.Models;
using StackExchange.Redis;

namespace Shared.Redis;
public sealed class RedisDb {
    public static RedisDb Instance { get; private set; }

    public ConnectionMultiplexer Redis;
    public IServer Server;
    public IDatabase Database;
    public ISubscriber Sub;

    public Global Globals;

    public Server Me;
    public readonly List<Server> Servers = [];
    public Server[] GameServers = [];
    public Server[] WebServers = [];

    public static int Population = 0;
    private RedisDb() { Instance = this; }
    public static RedisDb Create() => new RedisDb();
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
    public void SetMe(string name, string address, int port, int pop, int maxPop, int type) {
        Me = new Server(Database, name, type) {
            Name = name,
            IPAddress = address,
            Population = pop,
            MaxPopulation = maxPop,
            Port = port,
            ServerType = type,
        };

        var transaction = Database.CreateTransaction();
        Me.FlushAsync(transaction);

        Servers.Add(Me);
        Models.Servers.Add(Database, Me.Name, Me.ServerType);
        transaction.KeyExpireAsync(Me.Key, DateTime.Now + TimeSpan.FromMinutes(1));
        transaction.Execute();
    }

    public void UpdateMe(int pop) {
        if (Me == null)
            throw new Exception("Me (ServerInfo) is null");

        Me.Population = pop;

        var transaction = Database.CreateTransaction();
        Me.FlushAsync(transaction);
        transaction.KeyExpireAsync(Me.Key, DateTime.Now + TimeSpan.FromMinutes(1));
        transaction.Execute();
    }

    public void LoadGlobals() {
        Globals = new Global(Database);
        if(Globals.NextAccountId == 0)
            Globals.NextAccountId = 0;
        if (Globals.NextGuildId == 0)
            Globals.NextGuildId = 0;
        if (Globals.NextItemId == 0)
            Globals.NextItemId = 0;

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

    public bool IsNameTaken(string name) 
        => Names.Exists(Database, name);

    public bool TryLogin(string email, string password) {
        if (!Login.TryGetLogin(Database, email, out var login))
            return false;

        var hashed = Convert.ToBase64String((password + login.Salt).ToSHA512());

        if (hashed != login.HashedPassword)
            return false;

        return true;
    }
    //This method assumes you already checked if a account exists with the same email
    public Account CreateAccount(string email, string password, string name) {
        var newId = Globals.NextAccountId++;
        var trans = Database.CreateTransaction();
        Names.Add(Database, name, newId);
        var login = Login.Create(Database, email, password, newId);
        var account = new Account(Database, login.AccountId) { 
            Name = name,
            Rank = Rank.None,
            Id = newId,
            NextCharId = 0,
            GuildId = 0,
            RegisteredTime = DateTime.UtcNow,
            LastOnlineTime = DateTime.UtcNow,
            Banned = false,
            Alive = [],
        };

        account.FlushAsync(trans);
        Globals.FlushAsync(trans);
        trans.Execute();

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
    public void Dispose() {
        Database = null;
        Server = null;
        Redis = null;
        Sub = null;
        Globals = null;
        Me = null;
        Servers.Clear();
    }
}
