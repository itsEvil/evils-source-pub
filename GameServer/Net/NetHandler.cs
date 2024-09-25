using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;
using Shared;
using GameServer.Core.Options;
using Shared.Redis;
namespace GameServer.Net;
public class NetHandler
{
    public ConcurrentDictionary<int, Client> Clients = [];

    private bool Terminate = false;

    public int Backlog;
    public int MaxConnections;
    public int MaxConnectionsPerIp;

    private readonly Dictionary<string, int> _connections = [];
    private readonly Dictionary<string, List<int>> _ipToClientIds = [];
    private readonly Dictionary<string, DateTime> _bannedToUnbanTime = [];


    private readonly Queue<Client> _clientPool = [];
    private readonly Queue<Client> _toAddClient = [];
    private readonly Queue<int> _toRemoveClient = [];

    private List<Task> _tickTasks;
    private List<Task> _networkTasks;
    private IPEndPoint _endPoint;
    private Socket _listener;

    public void Init(AppOptions options)
    {
        Backlog = (int)options.Backlog;
        MaxConnections = (int)options.MaxConnections;
        MaxConnectionsPerIp = (int)options.MaxConnectionsPerIp;

        _networkTasks = new(MaxConnections);
        _tickTasks = new(MaxConnections);
        _endPoint = new(IPAddress.Any, (int)options.Port);
        _listener = new(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }
    public void AcceptConnections()
    {
        Thread.CurrentThread.IsBackground = true;

        for (int i = 0; i < MaxConnections; i++)
        {
            _clientPool.Enqueue(new Client(i, this));
        }

        _listener.Bind(_endPoint);
        _listener.Listen(Backlog);
        SLog.Info("Listening::On::{0}:{1}", _endPoint.Address.ToString(), 2050);

        while (!Terminate)
        {
            var socket = _listener.Accept();
            if (socket is null)
                continue;

            using var defer = new DeferAction(() => { TryCloseSocket(socket); });

            if (!_clientPool.TryDequeue(out var client))
            {
                SLog.Error("NoPooledClientsAvailable");
                continue;
            }

            if (socket.RemoteEndPoint is null)
            {
                SLog.Error("NoRemoteEndPoint");
                continue;
            }

            var ip = "unknown_endpoint";
            if (socket.RemoteEndPoint is IPEndPoint endpoint)
            {
                ip = endpoint.Address.ToString();
            }

            //var ip = socket.RemoteEndPoint?.ToString()?.Split(':')[0] ?? "unknown_endpoint";
            SLog.Debug("NewConnection::{0}", ip);
            if (_bannedToUnbanTime.TryGetValue(ip, out var banTime))
            {
                if (DateTime.Now - banTime > TimeSpan.Zero)
                {
                    _bannedToUnbanTime.Remove(ip);
                }
                else
                {
                    return;
                }
            }

            if (_connections.TryGetValue(ip, out var amount))
            {
                if (amount > MaxConnections)
                {
                    _bannedToUnbanTime[ip] = DateTime.Now + TimeSpan.FromSeconds(30);
                    KickClients(ip);
                    return;
                }
                _connections[ip] += 1;
            }
            else
            {
                _connections[ip] = 1;
            }

            if (_ipToClientIds.TryGetValue(ip, out var ids))
            {
                ids.Add(client.Id);
            }
            else
            {
                ids = [client.Id];
                _ipToClientIds[ip] = ids;
            }

            client.Begin(socket);
            _toAddClient.Enqueue(client);
            defer.Cancel = true;
        }
    }
    public void AddBack(Client client)
    {
        _toRemoveClient.Enqueue(client.Id);
        _clientPool.Enqueue(client);
    }
    private void KickClients(string ip)
    {
        if (!_ipToClientIds.TryGetValue(ip, out var ids))
        {
            SLog.Debug("NoConnectedClientsFoundFor::{0}", ip);
            return;
        }

        foreach (var id in ids)
        {
            if (Clients.TryGetValue(id, out var client))
            {
                client.LateDisconnect = true;
            }
        }
    }
    private static void TryCloseSocket(Socket socket)
    {
        try
        {
            socket.Close();
        }
        catch (Exception e)
        {
            SLog.Error(e);
        }
    }

    public async void NetworkTick()
    {
        Thread.CurrentThread.IsBackground = true;

        while (!Terminate)
        {
            while (_toRemoveClient.TryDequeue(out var id))
                Clients.Remove(id, out _);

            while (_toAddClient.TryDequeue(out var client))
                Clients[client.Id] = client;

            foreach (var (_, client) in Clients)
                _networkTasks.Add(client.NetworkTick());

            await Task.WhenAll(_networkTasks);
            _networkTasks.Clear();

            Thread.Sleep(Time.PerNetworkTick);
            Time.NetworkTickCount++;
        }
    }

    public async void Tick() {
        RedisDb.Population = Clients.Count;

        foreach (var (_, client) in Clients)
            _tickTasks.Add(client.Tick());

        await Task.WhenAll(_tickTasks);
        _tickTasks.Clear();
    }

}
