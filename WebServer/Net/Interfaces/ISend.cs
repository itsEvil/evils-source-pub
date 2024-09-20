using Shared;
using Shared.Interfaces;

namespace WebServer.Net.Interfaces;
public interface ISend : IWriteable
{
    public ushort Id { get; }
}
