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
    public void SendError(string message) { SLog.Debug("Player::SendError::{0}", message); }
    public void SendInfo(string message) { SLog.Debug("Player::SendInfo::{0}", message); }
    public void SendHelp(string message) { SLog.Debug("Player::SendHelp::{0}", message); }
}