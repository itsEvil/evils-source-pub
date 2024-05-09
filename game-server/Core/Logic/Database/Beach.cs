using game_server.Core.Logic.Transitions;

namespace game_server.Core.Logic.Database;

//Note
//
//These IBehaviourDatabase classes do not
//automtically get added as I don't want to use reflection

//Check at the top of Behavior.cs if you want to add new IBehaviourDatabase classes
public sealed partial class Beach : IBehaviourDatabase {
    public void Init() {
        BehaviorDb.Init("Pirate", 
            new State("Start",
                new TimedTransition(1000, "Test1-c"),
                new State("Test1",
                    new State("Test1-a"),
                    new State("Test1-b"),
                    new State("Test1-c", new TimedTransition(1000, "Test2")),
                    new State("Test1-d", 
                        new State("Test2",
                            new TimedTransition(1000, "Start")
                            )
                        )
                    )
                ),
            new State("Last")
            );
    }
}
