using WebServer.Core;
using WebServer.Core.Options;

namespace WebServer;
public sealed class Program {
    static int Main(string[] args) {
        var b = Builder.Create();

        b.AddOptions<AppOptions>((options) => { 
        
        
        });

        using var app = b.Build();

        return app.Run();
    }
}
