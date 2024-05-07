using common.db;
using common.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace common;
public class WorldDesc(WorldTypes worldType, string worldName) {
    public readonly WorldTypes WorldType = worldType;
    public readonly string WorldName = worldName;
}

public class ObjectDesc {
    public readonly string Name;
    public readonly ObjectClass Class;
    public readonly int Id;
    public readonly int MaximumHp;
    public ObjectDesc(XElement xml, int id, string name, ObjectClass @class = ObjectClass.GameObject) {
        Name = name;
        Id = id;
        MaximumHp = xml.ParseInt("MaximumHp", 100);
        Class = @class;
    }
}

public class PlayerDesc : ObjectDesc {
    public const int InventorySize = 20; //4 equipment, 8 inventory, 8 backpack 
    public const int PotionsSize = 2;
    public readonly ItemTypes[] SlotTypes;
    public readonly int[] Equipment;
    public ItemModel[] StartingItems = []; //Loaded after all items are loaded
    public PotionModel[] StartingPotions = []; //Loaded after all items are loaded
    public readonly Stat[] Stats = [];
    public readonly UnlockClass? Unlock;
    public PlayerDesc(XElement xml, int id, string name) : base(xml, id, name, ObjectClass.Player) {
        //Starting Item Types
        Equipment = xml.ParseIntArray("Equipment", ',', []);

        //Parse the slot types for the class up to the size of the inventory set above
        //If size of the SlotTypes array is smaller then InventorySize it sets it to
        //Any slot, same thing happens with StartingItems but as a Empty Item
        var itemTypes = xml.ParseIntArray("SlotTypes", ',', []);
        List<ItemTypes> types = [];
        for(int i = 0; i < InventorySize; i++) {
            if (i >= itemTypes.Length) {
                types.Add(ItemTypes.Any);
            } else {
                types.Add((ItemTypes)itemTypes[i]);
            }
        }
        SlotTypes = [.. types];

        //Parse potions that the class will start out with, not checked if these exists as items
        StartingPotions = new PotionModel[PotionsSize];
        var p = xml.Element("Potions");
        if (p != null) {
            int i = 0;
            foreach(var pot in p.Elements("Potion")) {
                if (i >= PotionsSize)
                    break;

                var type = pot.ParseInt("@type", -1);
                var count = pot.ParseInt("@count", 0);
                StartingPotions[i++] = new PotionModel(type, count);
            }
        } else {
            for(int i = 0; i < PotionsSize; i++) {
                StartingPotions[i] = PotionModel.Empty;
            }
        }

        Stats = new Stat[8];
        for (var i = 0; i < Stats.Length; i++)
            Stats[i] = new Stat(i, xml);

        if (xml.Element("UnlockLevel") != null || xml.Element("UnlockCost") != null)
            Unlock = new UnlockClass(xml);

    }
    public IEnumerable<int> GetStartStats() {
        for (int i = 0; i < Stats.Length; i++)
            yield return Stats[i].StartingValue;
    }

    //This will prevent the need of all the -1's in the xmls, just setup the items you want to give players
    public ItemModel[] CreateInventory(int[] items)
    {
        var ret = new ItemModel[InventorySize];
        for (int i = 0; i < ret.Length; i++)
        {
            if (i >= items.Length)
            {
                ret[i] = ItemModel.Empty;
            }
            else
            {
                var item = Resources.IdToItemDesc[items[i]];

                ret[i] = new ItemModel(item.Id, item.DisplayName, item.Stats, item.Soulbound);
            }
        }

        StartingItems = ret;
        return ret;
    }
}

public class ItemDesc(XElement xml, int id, string name) : ObjectDesc(xml, id, name, ObjectClass.Equipment) {
    public readonly int[] Stats = [];
    public readonly bool Soulbound = xml.ParseBool("Soulbound", false);
    public readonly string DisplayName = xml.ParseString("DisplayName", name);
}

public class ProjectileDesc(XElement xml, int id, string name) : ObjectDesc(xml, id, name, ObjectClass.Projectile) {

}

public class GroundDesc(XElement xml, int id, string name) {
    public readonly int Id = id;
    public readonly string Name = name;
    public readonly bool NoWalk = xml.ParseBool("NoWalk", false);
    public readonly bool Sink = xml.ParseBool("Sink", false);
    public readonly float Speed = xml.ParseFloat("Speed", 1f);
}

//Copied from rotf:r source with some modifications
//rotf:r at the moment of writing this is based on TK source 
public class Stat
{
    public readonly int MaxIncrease;
    public readonly int MaxValue;
    public readonly int MinIncrease;
    public readonly int StartingValue;

    public string Type;

    public Stat(int index, XElement e) {
        Type = StatIndexToName(index);

        var x = e.Element(Type);

        if (x != null) {
            StartingValue = int.Parse(x.Value);
            MaxValue = x.ParseInt("@max");
        }

        var y = e.Elements("LevelIncrease");

        foreach (var s in y) {
            if (s.Value == Type) {
                MinIncrease = s.ParseInt("@min");
                MaxIncrease = s.ParseInt("@max");
                break;
            }
        }
    }
    public static string StatIndexToName(int index) {
        return index switch
        {
            0 => "MaxHitPoints",
            1 => "MaxMagicPoints",
            2 => "Attack",
            3 => "Defense",
            4 => "Speed",
            5 => "Dexterity",
            6 => "HpRegen",
            7 => "MpRegen",
            _ => "NotFound",
        };
    }
}

public class UnlockClass {
    public readonly int Cost;
    public readonly int Level;
    public readonly int Type;
    public UnlockClass(XElement e) {
        var n = e.Element("UnlockLevel");

        if (n != null && n.Attribute("type") != null && n.Attribute("level") != null) {
            Type = n.ParseInt("@type", -1);
            Level = n.ParseInt("@level", -1);
        }

        n = e.Element("UnlockCost");

        if (n != null)
            Cost = int.Parse(n.Value);
    }
}