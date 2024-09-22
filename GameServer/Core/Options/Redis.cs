using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core.Options;
public sealed class Redis {
    public string Host = "127.0.0.1";
    public string Password = "";
    public int Port = 6379;
    public int SyncTimeout = 60_000;
    public int Index = 0;
}
