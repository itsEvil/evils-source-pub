using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace common.db;
public class CharacterModel(AccountModel acc, int charId, bool isAsync = false) : RedisObject(acc.Database, $"char.{acc.Id}.{charId}", null, isAsync) {
    public AccountModel Account { get; private set; } = acc;
    public int Id { get; set; } = charId;
    public int ObjectType { get => GetValue<int>("type"); set => SetValue<int>("type", value); }
    public int Level { get => GetValue<int>("level"); set => SetValue<int>("level", value); }
    public int Experience { get => GetValue<int>("experience"); set => SetValue<int>("experience", value); }
    public int Fame { get => GetValue<int>("fame"); set => SetValue<int>("fame", value); }
    public int Skin { get => GetValue<int>("skin"); set => SetValue<int>("skin", value); }
    public int[] Stats { get => GetValue<int[]>("stats"); set => SetValue<int[]>("stats", value); }
    public bool Dead { get => GetValue<bool>("dead"); set => SetValue<bool>("dead", value); }
    public DateTime CreationTime { get => GetValue<DateTime>("createTime"); set => SetValue("createTime", value); }
    public DateTime LastSeen { get => GetValue<DateTime>("seenTime"); set => SetValue("seenTime", value); }
    public PotionModel[] PotionStackTypes { get => GetValue<PotionModel[]>("potions"); set => SetValue<PotionModel[]>("potions", value); }
    public ItemModel[] Inventory { get => GetValue<ItemModel[]>("inventory", []); set => SetValue<ItemModel[]>("inventory", value); }
}
