using Shared;
using Shared.Interfaces;

namespace GameServer.Net.Interfaces;
public interface ISend : IWriteable
{
    public ushort Id { get; }
}
