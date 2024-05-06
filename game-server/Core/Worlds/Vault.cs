using common;
using game_server.Networking;

namespace game_server.Core.Worlds;
public sealed class Vault(int id) : World(id) {
    public List<int> AllowedIds = []; //Change to a HashSet<int> for worlds where you allow a lot of Clients (50+) that can't be easily sorted out
    public override void Init(WorldDesc descriptor, Client? client = null) {
        if (client is not null)
            AllowedIds.Add(client.AccountId);
    }
    public override bool IsAllowedAccess(Client client) {
        return AllowedIds.Contains(client.AccountId) || client.Account?.Rank >= Ranks.Owner;
    }
}
