using WebServer.Core;
using WebServer.Core.Options;

namespace WebServer;
public sealed class Program {
    static int Main(string[] args) {
        var builder = Builder.Create();

        builder.AddOptions<AppOptions>((options) => { 
        
        
        });

        using var app = builder.Build<Application>();

        return app.Run();
    }
}
