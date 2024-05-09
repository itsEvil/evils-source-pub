using common;
using common.db;
using game_server.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Entities;

//Stats Manager to keep track of active boosts
public sealed partial class Player {
    public Task SendUpdate()
    {
        return Task.CompletedTask;
    }
    public Task SendNewTick()
    {
        return Task.CompletedTask;
    }
}