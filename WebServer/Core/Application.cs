using common;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServer.Core.Options;

namespace WebServer.Core;
public class Application : IDisposable {
    private bool _terminate = false;
    private AppOptions _options;
    private NetHandler _handler;
    public Application()
    {
        _handler = new();
    }
    public void Awake(AppOptions options) {
        _options = options;

        _handler.Init(options);
        Task.Run(_handler.AcceptConnections);
    }
    public int Run() {
        Task.Run(_handler.NetworkTick);
        
        while (!_terminate) {
            //var threads = Process.GetCurrentProcess().Threads;
            //SLog.Debug("Threads: {0}", args: [threads.Count]);
            _handler.Tick();

            Thread.Sleep(1);
            //move to seperate thread
        }

        return 0;
    }
    public void Stop() {
        _terminate = true;
    }
    public void Tick() {

    }
    public void Dispose() {

    }
}
