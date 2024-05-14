using common;
using common.db;
using Microsoft.AspNetCore.Mvc;
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
        app.Use((context, next) =>
        {
            SLog.Debug("Request::{0}::From::{1}", context.Request.Path, context.Request.GetIp());
            return next(context);
        });

        MapAppGroup(app);
        MapAccountGroup(app);
        MapCharacterGroup(app);
        MapClientError(app);

        SLog.Info("Listening at {0}", url);
        app.Run();
    }

    private static void MapClientError(WebApplication app)
    {
        //Probably want to use Antiforgery but meh
        var errorGroup = app.MapGroup("/clientError").DisableAntiforgery();
        errorGroup.MapPost("/add", (HttpContext context) => {
            SLog.Debug("ClientError::{0}", string.Join(',', context.Request.Form.Keys));
            SLog.Debug("ClientError::{0}::{1}", context.Request.Form["text"], context.Request.Form["guid"]);
        });
    }

    private static void MapCharacterGroup(WebApplication app) {
        var charGroup = app.MapGroup("/char").DisableAntiforgery();
        charGroup.MapPost("/list", async (HttpContext context) => {
            SLog.Info("CharListRequest::{0}", string.Join(',', context.Request.Form.Keys));
            var form = context.Request.Form;

            if(!form.TryGetValue("email", out var emailPrim) || !form.TryGetValue("password", out var passwordPrim)) {
                var guest = await Redis.CreateGuestAccountAsync("guest@email.com");
                return Results.Ok(new CharListResponse(guest.Id, guest.Name, (int)guest.Rank, guest.Credits, guest.NextCharSlotCurrency, guest.NextCharSlotCurrency, guest.Skins));
            }

            var email = emailPrim.ToString();
            var password = passwordPrim.ToString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)) {
                var guest = await Redis.CreateGuestAccountAsync("guest@email.com");
                return Results.Ok(new CharListResponse(guest.Id, guest.Name, (int)guest.Rank, guest.Credits, guest.NextCharSlotCurrency, guest.NextCharSlotCurrency, guest.Skins));
            }

            (LoginStatus, AccountModel?) results = await Redis.VerifyAsync(email, password);
            if (results.Item1 == LoginStatus.Failed) {
                var guest = await Redis.CreateGuestAccountAsync("guest@email.com");
                return Results.Ok(new CharListResponse(guest.Id, guest.Name, (int)guest.Rank, guest.Credits, guest.NextCharSlotCurrency, guest.NextCharSlotCurrency, guest.Skins));
            }
            var acc = results.Item2;
            if(acc is null) {
                var guest = await Redis.CreateGuestAccountAsync("guest@email.com");
                return Results.Ok(new CharListResponse(guest.Id, guest.Name, (int)guest.Rank, guest.Credits, guest.NextCharSlotCurrency, guest.NextCharSlotCurrency, guest.Skins));
            }
            
            return Results.Ok(new CharListResponse(acc.Id, acc.Name, (int)acc.Rank, acc.Credits, acc.NextCharSlotPrice, acc.NextCharSlotCurrency, acc.Skins));
        });

    }

    private static void MapAccountGroup(WebApplication app) {

        var accGroup = app.MapGroup("/account").DisableAntiforgery();
        accGroup.MapPost("/verify", async (Verify verify) => {
            if (string.IsNullOrEmpty(verify.Email) || string.IsNullOrEmpty(verify.Password)) {
                return Results.BadRequest();
            }

            (LoginStatus, AccountModel?) results = await Redis.VerifyAsync(verify.Email, verify.Password);
            if (results.Item1 == LoginStatus.Failed)
                return Results.Ok("BadLogin");

            var acc = results.Item2;
            if (acc is null) {
                SLog.Warn("LoginStatus::{0}::ButAccountIsNull::Email::{1}", results.Item1, verify.Email);
                return Results.Ok("BadLogin");
            }
            //todo return Account.ToJson();
            return Results.Ok(new VerifyResponse(acc.Id, acc.Name, (int)acc.Rank, acc.Credits, acc.NextCharSlotPrice, acc.NextCharSlotCurrency, acc.Skins));
        });

        accGroup.MapPost("/register", async (HttpContext context,[FromForm] string email, [FromForm] string password, [FromForm] string username) =>
        {
            SLog.Debug("Register:{0}", email);
            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(username)) {
                return Results.BadRequest();
            }
            
            var ip = context.Request.GetIp();
            SLog.Debug("ip:{0}", ip);
            var result = await Redis.RegisterAsync(email, password, username, ip);
            SLog.Debug("result:{0}", result);

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

[JsonSerializable(typeof(CharListResponse))]
[JsonSerializable(typeof(VerifyResponse))]
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
public class CharListResponse(int accId, string name, int rank, int credits, int nextCharSlotPrice, int nextCharSlotCurrency, int[] skins, string menuMusic = "none", string deadMusic = "none")
{
    public int AccountId { get; set; } = accId;
    public string Name { get; set; } = name;
    public int Rank { get; set; } = rank;
    public int Credits { get; set; } = credits;
    public int NextCharSlotPrice { get; set; } = nextCharSlotPrice;
    public int NextCharSlotCurrency { get; set; } = nextCharSlotCurrency;
    public string MenuMusic { get; set; } = menuMusic;
    public string DeadMusic { get; set; } = deadMusic;
    public int[] Skins { get; set; } = skins;
    public override string ToString()
    {
        return $"VerifyResponse:[{AccountId}|{Name}]";
    }
}

public class VerifyResponse(int accId, string name, int rank, int credits, int nextCharSlotPrice, int nextCharSlotCurrency, int[] skins, string menuMusic = "none", string deadMusic = "none" ) {
    public int AccountId { get; set; } = accId;
    public string Name { get; set; } = name;
    public int Rank { get; set; } = rank;
    public int Credits { get; set; } = credits;
    public int NextCharSlotPrice { get; set; } = nextCharSlotPrice;
    public int NextCharSlotCurrency { get; set; } = nextCharSlotCurrency;
    public string MenuMusic { get; set; } = menuMusic;
    public string DeadMusic { get; set; } = deadMusic;
    public int[] Skins { get; set; } = skins;
    public override string ToString() {
        return $"VerifyResponse:[{AccountId}|{Name}]";
    }
}

public record InitData(int PotionPrice = 50, int VaultSlotPrice = 250, int CharSlotPrice = 1000);
public class ServerInfo(string name, string address, int port) {
    public string Name { get; set; } = name;
    public string Address { get; set; } = address;
    public int Port { get; set; } = port;
}