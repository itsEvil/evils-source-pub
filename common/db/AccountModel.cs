using common.utils;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common.db;
public class AccountModel : RedisObject {
    public AccountModel(IDatabase db, int accountId, string? field = null, bool isAsync = false) 
        : base(db, "account." + accountId, field, isAsync) {

        Id = accountId;

        if (isAsync || field != null)
            return;

        var time = Utils.FromUnixTimestamp(BanLiftTime);
        if (!Banned || BanLiftTime <= -1 || time > DateTime.UtcNow)
            return;

        Banned = false;
        BanLiftTime = 0;
        FlushAsync();
    }

    public int Id { get; private set; } 
    public string Name { get => GetValue<string>("name"); set => SetValue<string>("name", value); }
    public string Email { get => GetValue<string>("email"); set => SetValue<string>("email", value); }
    public int NextCharId { get => GetValue<int>("nextCharId", 0); set => SetValue<int>("nextCharId", value); }
    public int BanLiftTime { get => GetValue<int>("banLiftTime"); set => SetValue<int>("banLiftTime", value); }
    public bool Banned { get => GetValue<bool>("banned"); set => SetValue<bool>("banned", value); }
    public Ranks Rank { get => (Ranks)GetValue<int>("rank", 0); set => SetValue<int>("rank", (int)value); }
    public DateTime RegTime { get => GetValue<DateTime>("regTime"); set => SetValue("regTime", value); }
    public string IP { get => GetValue<string>("ip"); set => SetValue("ip", value); }
    public ItemModel[] Gifts { get => GetValue<ItemModel[]>("gifts", []); set => SetValue("gifts", value); }
}
