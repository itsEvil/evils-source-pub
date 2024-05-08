using common;
using game_server.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Logic;
public class Transition(params string[] targetStates) : IStateChild {
    public readonly int Id = ++BehaviorDb.NextId;
    public string[] StringTargetStates = [.. targetStates.Select(x => x.ToLower())]; //Only used for parsing.
    public readonly List<int> TargetStates = [];
    public int GetTargetState() => TargetStates[Random.Shared.Next(TargetStates.Count)];
    public virtual void Enter(Entity host) { 
        //SLog.Debug("Trans::Enter::{0}", host.Name);
    }
    public virtual bool Tick(Entity host) {
        //SLog.Debug("Trans::Tick::{0}", host.Name);
        return false;    
    }
    public virtual void Exit(Entity host) { 
        //SLog.Debug("Trans::Exit::{0}", host.Name);
    }
}
