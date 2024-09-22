namespace Shared;
public static class Time {
    public static int GameTickCount = 0;
    public static int NetworkTickCount = 0;
    public const int PerGameTick = 1000 / GameTps;
    public const int PerNetworkTick = 1000 / NetworkTps;
    public const int GameTps = 10;
    public const int NetworkTps = 20;
}
