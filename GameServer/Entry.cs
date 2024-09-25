using GameServer.Core;
using GameServer.Core.Options;
using Shared;
namespace GameServer;
public static class Program {
    public static int Main(string[] args) {
        var b = Builder.Create();

        b.AddOptions<AppOptions>((options) => {
            options.Name = "Local - GameServer";
            options.Port = 2050;
        });

        using var app = b.Build();

        var exitCode = app.Run();

        SLog.Error("Closed with exit code {0}\n\tPress any key to close...", exitCode);
        Console.ReadLine();
        return exitCode;
    }
}
