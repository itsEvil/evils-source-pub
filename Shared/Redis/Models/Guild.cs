using StackExchange.Redis;

namespace Shared.Redis.Models;
internal class Guild : RedisObject
{
    public uint Id = 0;
    public string Name = "";
    public string Description = "";
    public uint[] Members = [];

    public Guild(IDatabase db, int id, string field = null, bool isAsync = false) : base(db, "guild." + id, field, isAsync)
    {

    }

    public void Write(Writer w, Span<byte> buffer) {
        w.Write(buffer, Id);
        w.Write(buffer, Name);
        w.Write(buffer, Description);
        w.Write(buffer, (ushort)Members.Length);
        for (int i = 0; i < Members.Length; i++)
            w.Write(buffer, Members[i]);        
    }
}
