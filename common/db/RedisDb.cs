using common.utils;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pipelines.Sockets.Unofficial.Threading.MutexSlim;

namespace common.db;
public class RedisDb {
    private const int LockTTL = 15;
    private const string REG_LOCK = "regLock";
    private const string NAME_LOCK = "nameLock";
    private readonly ConnectionMultiplexer Redis;
    public readonly IDatabase Database;
    public readonly IServer Server;
    public readonly ISubscriber Sub;
    public RedisDb(string host = "127.0.0.1", int port = 6379, int index = 0, string auth = "") {
        var conString = host + ":" + port + ",syncTimeout=60000";
        if (!string.IsNullOrWhiteSpace(auth))
            conString += ",password=" + auth;

        Redis = ConnectionMultiplexer.Connect(conString); 
        Server = Redis.GetServer(Redis.GetEndPoints(true)[0]);
        Database = Redis.GetDatabase(index);
        Sub = Redis.GetSubscriber();

        SLog.Info("Connected to Redis DB");
    }
    public async Task<RegisterStatus> RegisterAsync(string email, string password, string username, string ip = "unknown_endpoint") {
        if (password.Length < 10)
            return RegisterStatus.InvalidPassword;

        if (!Utils.IsValidEmail(email))
            return RegisterStatus.InvalidEmail;

        if (username.Length > 12)
            return RegisterStatus.InvalidName;

        var emailUpper = email.ToUpperInvariant();
        if (!await Database.HashSetAsync("logins", emailUpper, "{}", When.NotExists)) {
            return RegisterStatus.EmailTaken;
        }

        using var removeLogin = new DeferAction(() => {
            //SLog.Debug("Removed::Email::{0}", emailUpper);
            Database.HashDelete("logins", emailUpper);
        });

        var usernameUpper = username.ToUpperInvariant();
        if (!await Database.HashSetAsync("names", usernameUpper, username, When.NotExists)) {
            return RegisterStatus.InvalidName;
        }

        using var removeName = new DeferAction(() => {
            //SLog.Debug("Removed::Named::{0}", usernameUpper);
            Database.HashDelete("names", usernameUpper);
        });

        var accId = (int)await Database.StringIncrementAsync("nextAccountId");
        var account = new AccountModel(Database, accId) {
            Email = email,
            Name = username,
            Rank = 0,
            NextCharId = 0,
            RegTime = DateTime.Now,
            MaximumCharSlots = 2,
            IP = ip,
            Gifts = [],
            Banned = false,
        };

        await account.FlushAsync();
        using var removeAccount = new DeferAction(() => {
            //SLog.Debug("Removed::Account::{0}", accId);
            Database.KeyDelete($"account.{accId}");
            Database.StringDecrement("nextAccountId");
        });

        var result = await DbLoginInfo.TryGetDbLoginInfoAsync(Database, email);
        DbLoginInfo info = result ?? new DbLoginInfo(0, "", "") { Email = email };

        var x = new byte[0x10];

        Utils.Generator.GetNonZeroBytes(x);

        var salt = Convert.ToBase64String(x);
        var hash = Convert.ToBase64String((password + salt).ToSHA256());

        info.HashedPassword = hash;
        info.Salt = salt;
        info.AccountId = accId;
        await info.FlushAsync();

        removeLogin.Cancel = true;
        removeName.Cancel = true;
        removeAccount.Cancel = true;
        return RegisterStatus.Ok;
    }

    public async Task<(CreateStatus, CharacterModel?)> CreateCharacterAsync(AccountModel account, int objectType, int skinType) {
        if(await Database.SetLengthAsync("alive." + account.Id) >= account.MaximumCharSlots) {
            return (CreateStatus.LimitReached, null);
        }

        //todo setup skins and check here if player has skin
        if(skinType != 0) {

        }

        if(!Resources.IdToObjectDesc.TryGetValue(objectType, out var objectDesc)) {
            SLog.Warn("RedisDb::Can't find object desc with id: {0}", objectType);
            return (CreateStatus.InvalidError, null);
        }

        if (!Resources.IdToPlayerDesc.TryGetValue(objectType, out var playerDesc)) {
            SLog.Warn("RedisDb::Can't find player desc with id: {0}", objectType);
            return (CreateStatus.InvalidError, null);
        }

        var newId = (int)await Database.HashIncrementAsync(account.Key, "nextCharId");

        var charModel = new CharacterModel(account, newId) {
            ObjectType = objectType,
            Level = 1,
            Experience = 0,
            Fame = 0,
            CreationTime = DateTime.Now,
            Dead = false,
            Skin = skinType <= 0 ? 0 : skinType,
            Inventory = [.. playerDesc.StartingItems],
            Stats = [.. playerDesc.GetStartStats()],
            PotionStackTypes = [.. playerDesc.StartingPotions],
            LastSeen = DateTime.Now,
        };

        await charModel.FlushAsync();
        await Database.SetAddAsync("alive." + account.Id, BitConverter.GetBytes(newId));

        return (CreateStatus.Ok, charModel);
    }
}
