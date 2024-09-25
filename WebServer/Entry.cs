using Shared;
using WebServer.Core;
using WebServer.Core.Options;

namespace WebServer;
public sealed class Program {
    static int Main(string[] args) {
        //Todo?
        //Add a args parser?
        //Add a json options file?
        
        var b = Builder.Create();

        b.AddOptions<AppOptions>((options) => {
            options.Name = "Local - WebServer";
            options.Port = 8080;
        });

        using var app = b.Build();

        var exitCode = app.Run();

        SLog.Error("Closed with exit code {0}\n\tPress any key to close...", exitCode);
        Console.ReadLine();
        return exitCode;
    }
}
