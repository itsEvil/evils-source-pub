using Shared;
using Shared.Redis;
using Shared.TimedSystems;
using System.Diagnostics;
using WebGui.Components;

namespace WebGui;
public class Program
{
    public static readonly RedisDb Redis = RedisDb.Create();
    private static readonly RedisUpdater m_Updater = new();


    public static void Main(string[] args)
    {
        Redis.Init("127.0.0.1", 6379, 30_000, 0, "");
        Redis.SetMe("WebGui", "127.0.0.1", 6969, -1, -1, 2);

        var builder = WebApplication.CreateBuilder(args);


        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        //todo add config
        //var port = 6969; //Config.serverInfo.port;
        //var address = "127.0.0.1"; //Config.serverInfo.bindAddress;
        //var url = $"https://{address}:{port}";
        //
        //app.Urls.Clear();
        //app.Urls.Add(url);

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Use((context, next) => {
            var currentTime = Stopwatch.GetTimestamp();
            SLog.Debug("Middleware - {0}", args: [currentTime]);

            var delta = Stopwatch.GetElapsedTime(m_Updater.LastRunTime);
            if(delta.Ticks >= m_Updater.Cooldown) {
                SLog.Debug("Middleware - {0}/{1}", args: [m_Updater.LastRunTime, currentTime]);
                m_Updater.LastRunTime = currentTime;
                m_Updater.Execute();

                foreach(var server in Redis.Servers)
                {
                    SLog.Debug("Middleware - Server - {0}", args: [server.Name]);
                }
            }

            return next(context);
        });

        app.Run();
    }
}
