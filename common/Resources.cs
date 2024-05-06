using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common;

public static class Resources {
    public readonly static Dictionary<string, WorldDesc> NameToWorldDesc = [];
    public readonly static Dictionary<string, ObjectDesc> NameToObjectDesc = [];
    public readonly static Dictionary<int, ObjectDesc> IdToObjectDesc = [];
    public readonly static Dictionary<string, PlayerDesc> NameToPlayerDesc = [];
    public readonly static Dictionary<int, PlayerDesc> IdToPlayerDesc = [];
    public static void InitXmls(string resourcePath = "") {
        if (string.IsNullOrEmpty(resourcePath)) {
            SLog.Debug("Resources::InitXmls::ResourcePathIsEmptyOrNull");
            return;
        }

        var dir = Directory.GetCurrentDirectory();
        SLog.Debug("Resources::InitXmls::Path::{0}::{1}", dir, resourcePath);


        var nexus = new WorldDesc(WorldTypes.Nexus, "Nexus");
        NameToWorldDesc[nexus.WorldName] = nexus;
        //Tests
        var wizard = new PlayerDesc(0, "Wizard");
        IdToObjectDesc[wizard.Id] = NameToObjectDesc["Wizard"] = IdToPlayerDesc[wizard.Id] = NameToPlayerDesc["Wizard"] = wizard;

        var pirate = new ObjectDesc(1, "Pirate");
        IdToObjectDesc[pirate.Id] = NameToObjectDesc["Pirate"] = pirate;
    }
}
