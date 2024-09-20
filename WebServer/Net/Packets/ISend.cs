using Shared;

namespace WebServer.Net.Packets;
public interface ISend
{
    public ushort Id { get; }
    public void Write(Writer writer, Span<byte> buffer);
}
