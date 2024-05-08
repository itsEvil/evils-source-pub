using common;

namespace game_server;
internal class Program {
    static async Task<int> Main(string[] _) {
        var cm = new CoreManager();
    
        while (!CoreManager.Terminate) {
            var amount = cm.Run();
            Thread.Sleep(await amount);
        }
    
        SLog.Warn("Program::Terminated::PressAnyKeyToClose");
        Console.ReadKey();
        return 0;
    }
}