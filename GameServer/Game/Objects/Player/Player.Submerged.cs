using Shared;

namespace GameServer.Game.Objects;
//All methods to do with submerged worlds
public partial class Player {
    public int Oxygen = 0;
    public void DrainHealth(int amount)
    {
        SLog.Error("TODO: {0} {1}", args: [World.Desc.Name, nameof(DrainHealth)]);
    }
    public void DrainResource(int amount)
    {
        SLog.Error("TODO: {0} {1}", args: [World.Desc.Name, nameof(DrainResource)]);
    }
}
