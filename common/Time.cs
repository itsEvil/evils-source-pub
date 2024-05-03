using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common;
public static class Time {
    public static int GameTickCount = 0;
    public static int NetworkTickCount = 0;
    public const int PerGameTick = 1000 / 10;
    public const int PerNetworkTick = 1000 / 20;
    public const int GameTps = 10;
    public const int NetworkTps = 20;
}
