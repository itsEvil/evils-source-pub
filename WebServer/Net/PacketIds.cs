namespace WebServer.Net;
//To
public enum C2S {
    Failure = 0,
    Hello = 1,
    Login = 2,
    Register = 3,
    Create = 4,
    CharList = 5,
    NewsList = 6,
}
//From
public enum S2C {
    Failure = 0,
    HelloAck = 1,
    LoginAck = 2,
    RegisterAck = 3,
    CreateAck = 4,
    CharListAck = 5,
    NewsListAck = 6,
}
