using common;
using game_server.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Logic;
public class Behaviour : IStateChild {
    [ThreadStatic]
    private static Random rand;
    protected static Random Random => rand ??= new Random();
    public virtual void Enter(Entity host) {
        SLog.Debug("Enter::{0}", host.Name);
    }
    public virtual void Exit(Entity host) { 
        SLog.Debug("Exit::{0}", host.Name);
    }
    public virtual void Tick(Entity host) { 
        SLog.Debug("Tick::{0}", host.Name);
    }
    public virtual void Death(Entity host) { 
        SLog.Debug("Death::{0}", host.Name);
    }
}
