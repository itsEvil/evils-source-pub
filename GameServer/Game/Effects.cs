using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game;
public enum ConditionEffect
{
    Mending = 0, //Flat Temporary Vitality Increase (+50 uncapped)
    Protection = 1, //% Increased defenses (+5% per stack)
    Slowness = 2, //% Reduced speed (-15% per stack. min 0)
    Swiftness = 3, //% Increased speed (+15% per stack)
    Berserk = 4, //% Increased attack speed (+10% per stack)
    Damaging = 5, //% Increased damage per hit (+10% per stack)

    Paralyzed = 6, //Flat Temporary reduced Speed (-50 speed. min 0)
}

public static class Effects
{
    public static readonly ConditionEffect[] Array = (ConditionEffect[])Enum.GetValues(typeof(ConditionEffect));
    public static float GetBaseValue(ConditionEffect effect)
    {
        return effect switch
        {
            ConditionEffect.Mending => 50,
            ConditionEffect.Protection => 5,
            ConditionEffect.Slowness => .15f,
            ConditionEffect.Swiftness => .15f,
            ConditionEffect.Berserk => .1f,
            ConditionEffect.Damaging => .1f,
            ConditionEffect.Paralyzed => 50,
            _ => 0,
        };
    }

    //Server side only...
    public static uint GetMaxStack(ConditionEffect effect)
    {
        return effect switch
        {
            ConditionEffect.Mending => 1,
            ConditionEffect.Protection => 10,
            ConditionEffect.Slowness => 5,
            ConditionEffect.Swiftness => 5,
            ConditionEffect.Berserk => 5,
            ConditionEffect.Damaging => 5,
            ConditionEffect.Paralyzed => 1,
            _ => 0,
        };
    }

}
