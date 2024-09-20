using Shared.Interfaces;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Redis.Models;
public sealed class Login {
    [JsonIgnore] public IDatabase Db;
    public uint AccountId { get; set; } = 0;
    public string Email { get; set; } = "";
    public string Salt { get; set; } = "";
    public string HashedPassword { get; set; } = "";
    public static bool TryGetLogin(IDatabase db, string email, out Login login) {
        login = null;
        var json = (string)db.HashGet("logins", email.ToUpperInvariant());
        if(json == null)
            return false;

        login = JsonSerializer.Deserialize<Login>(json, SourceGenerationContext.Default.Options);
        if (login is null)
            return false;
        
        login.Db = db;

        return true;
    }

    public static Login Create(IDatabase db, string email, string password, uint nextAccountId) {
        var x = new byte[0x10];

        Utils.Generator.GetNonZeroBytes(x);

        var salt = Convert.ToBase64String(x);
        var hash = Convert.ToBase64String((password + salt).ToSHA512());

        Login login = new() {
            Db = db,
            AccountId = nextAccountId,
            Email = email,
            Salt = salt,
            HashedPassword = hash
        };

        var isSet = db.HashSet("logins", email.ToUpperInvariant(), JsonSerializer.Serialize(login, SourceGenerationContext.Default.Options));
        //Shouldnt happen...
        if (!isSet) {
            SLog.Error("Login Field for {0} was overwritten!", args:[email.ToUpperInvariant()]);
        }

        return login;
    }
}
