using Shared;
using Shared.Redis.Models;
using WebServer.Core;
using WebServer.Net.Interfaces;

namespace WebServer.Net.Packets;

//Client requests the character list
public readonly struct NewsList : IReceive {
    public NewsList(Reader r, Span<byte> b) { }
    public void Handle(Client client) {
        if(client.Account == null || client.Rsa == null) {
            client.Tcp.EnqueueSend(new NewsListAck());
            return;
        }

        var redis = Application.Instance.Redis;
        var alive = redis.GetNews();

        client.Tcp.EnqueueSend(new NewsListAck(true, alive));
    }
}

public readonly struct NewsListAck(bool success = false, News[] news = null) : ISend
{
    public ushort Id => (ushort)S2C.NewsListAck;

    private readonly bool IsSuccess = success;
    private readonly News[] News = news;
    public void Write(Writer w, Span<byte> b)
    {
        w.Write(b, IsSuccess);
        if (IsSuccess) {
            w.Write(b, (ushort)News.Length);
            for (int i = 0; i < News.Length; i++)
                News[i].Write(w, b);
        }
    }
}
