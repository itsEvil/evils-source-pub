using Shared;

namespace GameServer.Game.Objects;
public partial class Player {
    public void Use(uint slot) {
        if (slot < 0 || slot >= Inventory.Length)
            return;

        SLog.Debug("Use item at slot {0}", args: [slot]);
    }
}
