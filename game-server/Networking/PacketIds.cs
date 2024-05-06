using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Networking;

public enum InPacket {
    Failure, //Disconnect
    Hello, //game version
    Login, //login details
    NewTickAck, //basically move packet
    Shoot,
    Hit,
    UpdateAck,
}

public enum OutPacket {
    Failure, //Disconnect
    HelloAck, //rsa key
    LoginAck, //result of login
    MoveAck, //
    ShootAck, 
    HitAck,
    Update,
    NewTick,
}