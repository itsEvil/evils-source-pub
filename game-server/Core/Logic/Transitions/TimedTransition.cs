using common;
using game_server.Core.Entities;

namespace game_server.Core.Logic.Transitions;
public sealed class TimedTransition(int time, string targetState) : Transition(targetState)
{
    private readonly int CooldownMS = time;
    public override void Enter(Entity host) {
        if (host.StateCooldown is null)
            return;

        host.StateCooldown[Id] = CooldownMS; //Id is the Transition Id
    }
    public override bool Tick(Entity host) {
        if (host.StateCooldown is null)
            return false;

        _ = base.Tick(host); //debug logging
        host.StateCooldown[Id] -= Time.PerGameTick;
        if (host.StateCooldown[Id] <= 0) {
            host.StateCooldown[Id] = CooldownMS;
            return true;
        }
        return false;
    }
    public override void Exit(Entity host) {
        if (host.StateCooldown is null)
            return;

        host.StateCooldown.Remove(Id);
    }

}
