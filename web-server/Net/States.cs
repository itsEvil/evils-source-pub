namespace WebServer.Net;
public sealed class SendState
{
    const int MaxSize = ushort.MaxValue;
    public readonly byte[] Data;
    public SendState()
    {
        Data = new byte[MaxSize];
    }
    public void Reset()
    {
        Data.AsSpan().Clear();
    }
}
public sealed class ReceiveState
{
    const int MaxSize = ushort.MaxValue;
    public readonly byte[] Data;
    public ReceiveState()
    {
        Data = new byte[MaxSize];
    }
    public void Reset()
    {
        Data.AsSpan().Clear();
    }
}