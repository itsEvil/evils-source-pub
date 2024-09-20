using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer.Core.Options;
public class AppOptions {
    public int Backlog = 1024;
    public int MaxConnections = 128;
    public int MaxConnectionsPerIp = 4;
    public int Port = 8080;

    public Redis Redis = new();
}
