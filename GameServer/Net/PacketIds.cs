namespace GameServer.Net;
//To
public enum C2S {
    Failure = 0,
    Hello = 1,
    Login = 2,
    Load = 3,
    TilesAck = 4,
    ObjectsAct = 5,
    DropsAck = 6,
    Move = 7,
}
//From
public enum S2C {
    Failure = 0,
    HelloAck = 1,
    LoginAck = 2,
    LoadAck = 3,
    Tiles = 4,
    Objects = 5,
    Drops = 6,
    MoveAck = 7,
}
