using common;
using common.db;
using System.Text.Json.Serialization;

namespace app_server;
public class Program {
    //Keep only 1 instance of these alive
    private static readonly InitData Init = new();
    private static readonly HashSet<ServerInfo> Servers = [];
    //Database
    private static readonly RedisDb Redis = new();
    public static void Main(string[] args) {

        var builder = WebApplication.CreateSlimBuilder(args);
        builder.Logging.ClearProviders();

        builder.Services.ConfigureHttpJsonOptions(options => {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            options.SerializerOptions.TypeInfoResolverChain.Insert(1, SourceGenerationContext.Default);
        });


        var app = builder.Build();

        //todo add config
        var port = 8080; //Config.serverInfo.port;
        var address = "127.0.0.1"; //Config.serverInfo.bindAddress;
        var url = $"http://{address}:{port}";

        app.Urls.Clear();
        app.Urls.Add(url);

        //Hardcoded server atm until I add a interserver messaging system
        Servers.Add(new ServerInfo("Localhost", "127.0.0.1", 2050));
        //To test Json
        //Servers.Add(new ServerInfo("Localhost-1", "127.0.0.1", 2051));
        //Servers.Add(new ServerInfo("Localhost-2", "127.0.0.1", 2052));
        
        app.Use((context, next) => {
            SLog.Debug("Request::{0}", context.Request.Path);
            return next(context);
        });

        MapAppGroup(app);
        MapAccountGroup(app);
        MapCharacterGroup(app);

        SLog.Info("Listening at {0}", url);
        app.Run();
    }

    private static void MapCharacterGroup(WebApplication app) {
        var charGroup = app.MapGroup("/char");
        
    }

    private static void MapAccountGroup(WebApplication app) {

        var accGroup = app.MapGroup("/account");
        accGroup.MapPost("/verify", (Verify verify) => {
            if (string.IsNullOrEmpty(verify.Email) || string.IsNullOrEmpty(verify.Password)) {
                return Results.BadRequest();
            }

            //todo return Account.ToJson();
            return Results.Ok();
        });

        accGroup.MapPost("/register", async (Register register) =>
        { 
            if(string.IsNullOrEmpty(register.Email) || string.IsNullOrEmpty(register.Password) || string.IsNullOrEmpty(register.Username)) {
                return Results.BadRequest();
            }

            SLog.Debug("{0}", register.ToString());
            var result = await Redis.RegisterAsync(register.Email, register.Password, register.Username);
            SLog.Debug("{0}", result);

            return result switch {
                RegisterStatus.InvalidError => Results.Ok("Invalid Error"),
                RegisterStatus.NameTaken => Results.Ok("Name Taken"),
                RegisterStatus.EmailTaken => Results.Ok("Email Taken"),
                RegisterStatus.InvalidEmail => Results.Ok("Invalid Email"),
                RegisterStatus.InvalidPassword => Results.Ok("Invalid Password"),
                RegisterStatus.InvalidName => Results.Ok("Invalid Name"),
                RegisterStatus.Ok => Results.Ok("Success"),
                _ => Results.Ok("Internal Error"),
            };
        });
    }

    private static void MapAppGroup(WebApplication app) {

        var appGroup = app.MapGroup("/app");
        appGroup.MapGet("/init", () => {
            return Results.Ok(Init);
        });

        appGroup.MapGet("/servers", () => {
            return Results.Ok(Servers);
        });
    }
}

[JsonSerializable(typeof(Verify))]
[JsonSerializable(typeof(Register))]
[JsonSerializable(typeof(HashSet<ServerInfo>))]
[JsonSerializable(typeof(ServerInfo))]
[JsonSerializable(typeof(InitData))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
public class Register(string email, string password, string username) {
    public string Email { get; set; } = email;
    public string Password { get; set; } = password;
    public string Username { get; set; } = username;
    public override string ToString() {
        return $"Register:[{Email}|{Password}|{Username}]";
    }
}
public class Verify(string email, string password) {
    public string Email { get; set; } = email;
    public string Password { get; set; } = password;
    public override string ToString() {
        return $"Verify:[{Email}|{Password}]";
    }
}

public record InitData(int PotionPrice = 50, int VaultSlotPrice = 250, int CharSlotPrice = 1000);
public class ServerInfo(string name, string address, int port) {
    public string Name { get; set; } = name;
    public string Address { get; set; } = address;
    public int Port { get; set; } = port;
}