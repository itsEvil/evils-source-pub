using common.utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace common;

public static class Resources {
    public readonly static Dictionary<string, WorldDesc> NameToWorldDesc = [];
    public readonly static Dictionary<string, ObjectDesc> NameToObjectDesc = [];
    public readonly static Dictionary<int, ObjectDesc> IdToObjectDesc = [];
    public readonly static Dictionary<string, PlayerDesc> NameToPlayerDesc = [];
    public readonly static Dictionary<int, PlayerDesc> IdToPlayerDesc = [];

    public readonly static Dictionary<int, ItemDesc> IdToItemDesc = [];
    public readonly static Dictionary<string, ItemDesc> NameToItemDesc = [];

    public readonly static Dictionary<int, ProjectileDesc> IdToProjectileDesc = [];
    public readonly static Dictionary<string, ProjectileDesc> NameToProjectileDesc = [];

    public readonly static Dictionary<int, GroundDesc> IdToGroundDesc = [];
    public readonly static Dictionary<string, GroundDesc> NameToGroundDesc = [];
    public static void InitXmls(string resourcePath = "") {
        if (string.IsNullOrEmpty(resourcePath)) {
            SLog.Debug("Resources::InitXmls::ResourcePathIsEmptyOrNull");
            return;
        }

        var dir = Directory.GetCurrentDirectory();
        var resources = Path.Combine(dir, resourcePath);
        SLog.Debug("Resources::InitXmls::Path::{0}", Path.Combine(dir, resourcePath));

        var fileNames = Directory.GetFiles(resources);
        foreach(var fileName in fileNames) {
            var file = File.ReadAllText(fileName);
            if (file is null)
                continue;


            var xml = XElement.Parse(file);
            foreach(var obj in xml.Elements("Object")) {
                var objectType = obj.ParseInt("@id", -1);
                var objectName = obj.ParseString("@name", "");
                var @class = obj.ParseEnum("Class", ObjectClass.GameObject);

                if(objectType == -1 || string.IsNullOrEmpty(objectName)) {
                    SLog.Error("Resources::SkippingObject::ObjectType Is -1 OR ObjectName IsNullOrEmpty");
                    continue;
                }

                ObjectDesc desc = @class switch {
                    ObjectClass.Player => NameToPlayerDesc[objectName] = IdToPlayerDesc[objectType] = new PlayerDesc(xml, objectType, objectName),
                    ObjectClass.Equipment => NameToItemDesc[objectName] = IdToItemDesc[objectType] = new ItemDesc(xml, objectType, objectName),
                    ObjectClass.Projectile => NameToProjectileDesc[objectName] = IdToProjectileDesc[objectType] = new ProjectileDesc(xml, objectType, objectName),
                    _ => new ObjectDesc(xml, objectType, objectName),
                };

                NameToObjectDesc[desc.Name] = IdToObjectDesc[desc.Id] = desc;
            }

            foreach(var ground in xml.Elements("Ground")) {
                var groundType = ground.ParseInt("@id", -1);
                var groundName = ground.ParseString("@name", "");

                NameToGroundDesc[groundName] = IdToGroundDesc[groundType] = new GroundDesc(ground, groundType, groundName);
            }
        }

        //Temp
        var nexus = new WorldDesc(WorldTypes.Nexus, "Nexus");
        NameToWorldDesc[nexus.WorldName] = nexus;

        SLog.Debug("Loaded::{0}::Objects", NameToObjectDesc.Count); //total amount
        SLog.Debug("Loaded::{0}::Classes", IdToPlayerDesc.Count);
        SLog.Debug("Loaded::{0}::Projectiles", IdToProjectileDesc.Count);
        SLog.Debug("Loaded::{0}::Items", IdToItemDesc.Count);
        SLog.Debug("Loaded::{0}::Grounds", IdToGroundDesc.Count);
    }
}
