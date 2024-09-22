using Shared.Interfaces;
using StackExchange.Redis;
using System.Data;
namespace Shared.Redis.Models;
public sealed class News : RedisObject, IWriteable {
    public readonly uint Id;
    public string Title { get => GetValue<string>("title"); set => SetValue("title", value); }
    public string ImageUrl { get => GetValue<string>("imageUrl"); set => SetValue("imageUrl", value); }
    public string ActionUrl { get => GetValue<string>("actionUrl"); set => SetValue("actionUrl", value); }
    public DateTime Start { get => GetValue<DateTime>("startTime"); set => SetValue("startTime", value); }
    public DateTime End { get => GetValue<DateTime>("endTime"); set => SetValue("endTime", value); }
    public News(IDatabase db, uint id, string field = null, bool isAsync = false) : base(db, "news." + id, field, isAsync) {
        Id = id;
    }
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, Title);
        w.Write(b, ImageUrl);
        w.Write(b, ActionUrl);
        w.Write(b, Start.ToBinary());
        w.Write(b, End.ToBinary());
    }
}
