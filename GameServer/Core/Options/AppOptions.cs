using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core.Options;
public class AppOptions {
    public int Backlog = 1024;
    public int MaxConnections = 128;
    public int MaxConnectionsPerIp = 4;
    public int Port = 8080;
    public int MaxPopulation = 100;

    public string Name = "Localhost";
    public string Address = "127.0.0.1";

    public Redis Redis = new();
    public Resources Resources = new("Data", "Game", "Web");
}
