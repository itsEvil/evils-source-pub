using common;
using game_server.Core.Entities;
using game_server.Core.Logic.Database;
using game_server.Core.Logic.Loots;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Logic;
public sealed class BehaviorDb {
    //Can't think of a better way that
    //doesn't use reflection until I figure out source generators
    public HashSet<IBehaviourDatabase> Database = [
        new Beach(), 
        new LowLands()
    ];


    public static int NextId;
    public static Dictionary<int, BehaviorModel> Models = [];
    public BehaviorDb() {
        foreach(var file in Database) {
            file.Init();
        }
        SLog.Info("Loaded::{0}::BehaviourModels", Models.Count);
    }
    public static void Init(string id, params IStateChild[] behaviors)
    {
        //SLog.Debug("Init::{0}::{1}", id, behaviors.Length);
        if(!Resources.NameToObjectDesc.TryGetValue(id, out var desc)) {
            SLog.Warn($"Skipping behaviour init for object '{id}' as it doesnt exist in resources!");
            return;
        }

#if DEBUG
        if (Models.ContainsKey(desc.Id))
            SLog.Warn($"Overwriting behavior for {desc.Name}");
#endif

        Models[desc.Id] = new BehaviorModel(behaviors);
    }

    public static BehaviorModel? Resolve(int type)
    {
        if (Models.TryGetValue(type, out var model))
            return model;

        return null;
    }
}

public sealed class BehaviorModel
{
    public Dictionary<int, State> States;
    public List<Behaviour> Behaviors;
    public Loot Loot;
    public State Root;
    public BehaviorModel(params IStateChild[] behaviors) {
        Behaviors = [];
        List<MobDrop> loots = [];
        Dictionary<int, State> temp = [];
        States = [];
        foreach (var bh in behaviors)
        {
            if (bh is null)
                continue;

            if (bh is MobDrop drop) 
                loots.Add(drop);
            if (bh is Threshold thresh) 
                loots.AddRange(thresh);
            if (bh is Behaviour behaviour)
                Behaviors.Add(behaviour);
            

            if (bh is State state) {
                temp.Add(state.Id, state);
            }
        }

        //SLog.Debug("StatesInTemp::{0}", string.Join(',', temp.Values));

        Root = temp.Values.First();
        States[Root.Id] = Root;


        Loot = new Loot([.. loots]);

        foreach (var s1 in temp.Values) {
            States = s1.GetAllStates(States);
        }

        foreach (var state in States.Values) {
            foreach (var t in state.Transitions) {
                foreach(var s in States.Values) {
                    if (t.StringTargetStates.Contains(s.StringId)) {
                        t.TargetStates.Add(s.Id);
                    }
                }

                //SLog.Debug("Transition::{0}::{1}", string.Join(',', t.StringTargetStates), string.Join(',', t.TargetStates));
            }
        }

        //SLog.Debug("BehaviourDb::{0}::{1}", States.Count, string.Join(',', States.Values));                    
    }
}
public interface IBehaviourDatabase {
    public abstract void Init();
}