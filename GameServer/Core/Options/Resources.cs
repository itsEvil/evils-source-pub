using Shared;

namespace GameServer.Core.Options;
public sealed class ResourcesOptions {
    public string DataPath;
    public string GameDataPath;
    public string WebDataPath;
    public ResourcesOptions(string data, string gameData, string webData) {
        var cur = Directory.GetCurrentDirectory();
        DataPath = data;
        GameDataPath = Path.Combine(cur, DataPath, gameData);
        WebDataPath = Path.Combine(cur, DataPath, webData);

        if (!Directory.Exists(DataPath))
            SLog.Error("Directory {0} does not exist!", args: [DataPath]);
        if (!Directory.Exists(GameDataPath))
            SLog.Error("Directory {0} does not exist!", args: [GameDataPath]);
        if (!Directory.Exists(WebDataPath))
            SLog.Error("Directory {0} does not exist!", args: [WebDataPath]);
    }
}
