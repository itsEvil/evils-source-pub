using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace common.db;

public class DbLoginInfo
{
    [JsonIgnore] private IDatabase db;

    [JsonConstructor]
    public DbLoginInfo(int accountId, string hashedPassword, string salt) {
        this.AccountId = accountId;
        this.HashedPassword = hashedPassword;
        this.Salt = salt;
    }
    public static bool TryGetDbLoginInfo(IDatabase db, string email, out DbLoginInfo info) {
        info = new DbLoginInfo(0, "", "") {
            Email = email
        };
        var json = (string?)db.HashGet("logins", email.ToUpperInvariant());

        if (json is null) {
            info.IsNull = true;
            return false;
        }

        var newInfo = JsonSerializer.Deserialize<DbLoginInfo>(json, SourceGenerationContext.Default.DbLoginInfo);
        if(newInfo is null) {
            info.IsNull = true;
            return false;
        }
        info = newInfo;
        info.db = db;
        info.Email = email;
        return true; 
    }
    public static async Task<DbLoginInfo?> TryGetDbLoginInfoAsync(IDatabase db, string email) {
        var json = (string?)await db.HashGetAsync("logins", email.ToUpperInvariant());

        if (json is null)
            return null;

        var info = JsonSerializer.Deserialize<DbLoginInfo>(json, SourceGenerationContext.Default.DbLoginInfo);
        if(info is null)
            return null;

        info.db = db;
        info.Email = email;
        return info;
    }
    [JsonIgnore] public string Email { get; set; } = "";
    [JsonIgnore] public bool IsNull { get; private set; } = true;
    public int AccountId { get; set; } = 0;
    public string HashedPassword { get; set; } = "";
    public string Salt { get; set; } = "";
    public void Flush() => db.HashSet("logins", Email.ToUpperInvariant(), JsonSerializer.Serialize(this, SourceGenerationContext.Default.DbLoginInfo));
}