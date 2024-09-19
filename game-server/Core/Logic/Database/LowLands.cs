﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Logic.Database;

//Note
//
//These IBehaviourDatabase classes are not
//automtically added as I don't want to use reflection

//Check at the top of Behavior.cs if you want to add new IBehaviourDatabase classes
public sealed class LowLands : IBehaviourDatabase {
    public void Init() {
        BehaviorDb.Init("Green Cube", 
            new State("Start")
        );
    }
}
