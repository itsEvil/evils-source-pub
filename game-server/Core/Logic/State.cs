using common;
using game_server.Core.Entities;

namespace game_server.Core.Logic;
public class State : IStateChild
{
    public string StringId; //Only used for parsing.
    public int Id;

    public State? Parent;
    public List<Behaviour> Behaviors;
    public List<Transition> Transitions;
    public Dictionary<int, State> States;

    public State(string id, params IStateChild[] behaviors)
    {
        StringId = id.ToLower();
        Id = ++BehaviorDb.NextId;

        Behaviors = [];
        Transitions = [];
        States = [];

        foreach (var bh in behaviors)
        {
#if DEBUG
            if (bh is Loot) 
                throw new Exception("Loot should not be initialized in a substate.");
#endif
            if (bh is Behaviour behaviour) 
                Behaviors.Add(behaviour);

            if (bh is Transition transition) 
                Transitions.Add(transition);

            if (bh is State state) {
                state.Parent = this;
                States.Add(state.Id, state);
            }
        }

        //SLog.Debug("States::{0}::{1}::{2}", States.Count, StringId, string.Join(',', States));
    }

    public void FindStateTransitions()
    {
        foreach (var transition in Transitions) {
            if (Parent is null)
                continue;

            foreach (var state in Parent.States.Values)
                if (transition.StringTargetStates.Contains(state.StringId))
                    transition.TargetStates.Add(state.Id);
        }

        foreach (var state in States.Values)
            state.FindStateTransitions();
    }
    //On entering state / world 
    public void Enter(Entity host) {
        host.CurrentState = this;

        Parent?.Enter(host);

        //SLog.Debug("State::Enter::{0}", host.Name);
        foreach(var behaviour in Behaviors)
            behaviour.Enter(host);

        foreach (var transitions in Transitions)
            transitions.Enter(host);
    }
    //On game tick
    public void Tick(Entity host) {
        //SLog.Debug("State::Tick::{0}", host.Name);

        Parent?.Tick(host);

        foreach (var behaviour in Behaviors)
            behaviour.Tick(host);

        foreach (var transition in Transitions) {
            if (transition.Tick(host)) {

                if(host.Behaviours is null) {
                    SLog.Error("Host.Behaviours is null for {0}::CurrentState::{1}", host.Name, StringId);
                    break;
                }    

                if (transition.TargetStates.Count == 0) {
                    SLog.Error("Transition for {0} has no target state ids::CurrentState::{1}", host.Name, StringId);
                    continue;
                }

                var id = transition.GetTargetState();
                if (host.Behaviours.States.TryGetValue(id, out var newState)) {
                    host.CurrentState?.Exit(host);
                    newState.Enter(host);
                    break;
                }

                SLog.Error("States::{0}::NoStateWithId::{1}", string.Join(',', host.Behaviours.States), id);

                break;
            }
        }
    }
    //On exiting state only via transition for example
    public void Exit(Entity host) {
        Parent?.Exit(host);
        //SLog.Debug("State::Exit::{0}", host.Name);
        foreach (var behaviour in Behaviors)
            behaviour.Exit(host);

        foreach (var transitions in Transitions)
            transitions.Exit(host);
    }
    //On when entity dies but doesn't leave world yet
    public void Death(Entity host) {
        SLog.Debug("State::Death::{0}", host.Name);
        Exit(host);
        Parent?.Death(host);

        foreach (var behaviour in Behaviors)
            behaviour.Death(host);
    }

    public Dictionary<int, State> GetAllStates(Dictionary<int, State> states) {
        foreach(var state in States) {
            states.TryAdd(state.Key, state.Value);
            state.Value.GetAllStates(states);
        }

        return states;
    }

    public override string ToString() {
        return $"{Id}:{StringId}";
    }
}
