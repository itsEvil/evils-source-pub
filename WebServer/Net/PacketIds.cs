namespace WebServer.Net;
//To
public enum C2S {
    Unknown = 0,
    Hello = 1,
    Login = 2,
    Register = 3,
}
//From
public enum S2C {
    Unknown = 0,
    HelloAck = 1,
    LoginAck = 2,
    RegisterAck = 3,
}
