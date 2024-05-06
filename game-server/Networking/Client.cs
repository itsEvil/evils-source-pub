using common;
using common.db;
using common.utils;
using game_server.Core.Entities;
using System.Buffers.Binary;
using System.Net.Sockets;

namespace game_server.Networking;
public sealed partial class Client(int id, CoreManager manager) {
    private const int LENGTH_PREFIX = 5;
    private const int LENGTH_PREFIX_WITH_ID = 4;

    public readonly CoreManager Manager = manager;

    public int Id = id;
    public int AccountId;
    public AccountModel? Account = null;
    public CharacterModel? Character = null;
    public Player? Player = null;

    private DateTime LastMessageTime;
    private DateTime DisconnectTime;

    private readonly SendState Send = new();
    private readonly ReceiveState Receive = new();
    private Socket? _socket;

    private readonly Queue<IOutPacket> OutPackets = [];
    private readonly Queue<IInPacket> InPackets = [];

    public bool ForceDisconnect = false;
    private CancellationTokenSource _source = new();
    private string IP = "unknown";
    public void BeginHandling(Socket socket, string ip) {
        _source = new();
        OutPackets.Clear();
        InPackets.Clear();
        Send.Reset();
        Receive.Reset();

        IP = ip;
        LastMessageTime = DateTime.Now;
        DisconnectTime = DateTime.Now + TimeSpan.FromMilliseconds(1000);

        _socket = socket;
        ReceiveAsync();
    }
    //Handled on Main Thread
    public Task Tick() {
        if (_socket is null || ForceDisconnect)
            return Task.CompletedTask;

        while(InPackets.TryDequeue(out var packet)) {
            packet.Handle(this);
        }

        return Task.CompletedTask;
    }

    //Handled on Network Thread
    public Task NetworkTick() {
        if(_socket is null) {
            Disconnect("SocketIsNull");
            return Task.CompletedTask;
        }

        if(DisconnectTime < DateTime.Now) {
            Disconnect($"TookTooLongToRespond::DCTime:{DisconnectTime}::Now:{DateTime.Now}");
            return Task.CompletedTask;
        }

        while(OutPackets.TryDequeue(out var packet)) {
            WritePacket(packet);
            Send.PacketsWritten++;
        }

        if (Send.PacketLength == 0)
            return Task.CompletedTask;

        FlushAsync();

        return Task.CompletedTask;
    }
    private void WritePacket(IOutPacket packet) {

        var buffer = Send.Buffer.AsSpan();
        int ptr = Send.PacketLength;
        
        ptr += LENGTH_PREFIX_WITH_ID;
        PacketUtils.WriteByte(buffer, packet.Id, ref ptr);
        
        var beforeWritePtr = ptr;
        packet.Write(buffer, ref ptr);
        var afterWritePtr = ptr;
        
        var packetSize = afterWritePtr - beforeWritePtr;
        var totalSize = packetSize + LENGTH_PREFIX;

        BinaryPrimitives.WriteInt32BigEndian(buffer[Send.PacketLength..(Send.PacketLength + LENGTH_PREFIX_WITH_ID)], totalSize);
        Send.PacketLength += totalSize;
    }
    private async void FlushAsync() {
        if (_socket is null)
            return;

        try {
            _ = await _socket.SendAsync(Send.Buffer.AsMemory(0, Send.PacketLength), SocketFlags.None, _source.Token);
        }
        catch (Exception e) {
            SLog.Warn("BufferLen: {0}, TotalLength: {1} PacketsWritten: {2}", Send.Buffer.Length, Send.PacketLength, Send.PacketsWritten);
            SLog.Error(e);
        }
        finally {
            Send.Reset();
        }
    }
    private async void ReceiveAsync()
    {
        if (_socket is null)
            return;

        try
        {
            while (_socket.Connected)
            {
                if (_socket is null || _source.IsCancellationRequested) {
                    return;
                }

                var len = await _socket.ReceiveAsync(Receive.Buffer.AsMemory(), _source.Token);

                if (len == 0) {
                    Disconnect("LengthReceiveIsZero");
                    break;
                }

                if (len > LENGTH_PREFIX_WITH_ID)
                    ProcessPacket(len);
            }
        }
        catch (Exception e) {
            Disconnect("CaughtException");
            if (e is not SocketException se || se.SocketErrorCode != SocketError.ConnectionReset &&
                se.SocketErrorCode != SocketError.Shutdown)
                SLog.Error($"Could not receive data from {Account?.Name ?? "[unconnected]"} ({IP}): {e}");
        }
    }
    public void Enqueue(IOutPacket packet) {
        OutPackets.Enqueue(packet);
    }
    private void ProcessPacket(int length)
    {
        try
        {
            int ptr = 0;
            var buffer = Receive.Buffer.AsSpan();
            while (ptr < length)
            {
                var packetLen = PacketUtils.ReadInt(buffer, ref ptr, length);
                var nextPacketPtr = ptr + packetLen - 4;
                var packetId = PacketUtils.ReadByte(buffer[ptr..], ref ptr, nextPacketPtr);

                //SLog.Info("Reading packet Id:{0} TotLen:{1} Len:{2} Next:{3}", packetId, length, packetLen, nextPacketPtr);
                ReadPacket((InPacket)packetId, buffer, ref ptr, nextPacketPtr);

                ptr = nextPacketPtr;
            }
        }
        catch (Exception e)
        {
            SLog.Error(e);
        }
        finally
        {
            Receive.Reset();
        }
    }
    private void ReadPacket(InPacket packetId, Span<byte> buffer, ref int ptr, int len) {
        //Read our packet
        IInPacket? packet = packetId switch
        {
            InPacket.Failure => new InFailure(buffer, ref ptr, len),
            _ => null,
        };

        //If we didn't find a packet make sure to log
        if (packet is null) {
            SLog.Warn("FailedToFindPacket::{0}", packetId);
            return;
        }

        LastMessageTime = DateTime.Now;
        DisconnectTime = DateTime.Now + TimeSpan.FromMilliseconds(3000); //Remember about that packets have to go to and come back from clients (RTT)

        //Enqueue to be handled on main thread
        InPackets.Enqueue(packet);
    }

    private void Disconnect(string message) {
        SLog.Debug("Disconnected::Client::{0}::For::{1}", Id, message);

        _source.Cancel();

        try
        {
            _socket?.Dispose();
        } 
#if DEBUG
        catch (Exception e)
        {
            SLog.Error(e);
        }
#endif
#if RELEASE
        catch { }
#endif

        if(Player is not null) {
            Player.SaveToCharacter();
            Player.Owner?.LeaveWorld(Player);
        }


        Account = null;
        Character = null;
        
        Manager.AddBack(this);
    }
}

public class SendState() {
    private const int BufferSize  = ushort.MaxValue;
    public readonly byte[] Buffer = new byte[BufferSize];
    public int PacketLength = 0;
    public int PacketsWritten = 0;
    public void Reset() {
        Buffer.AsSpan().Clear();
        PacketLength = 0;
        PacketsWritten = 0;
    }
}

public class ReceiveState() {
    private const int BufferSize = 4096;
    public readonly byte[] Buffer = new byte[BufferSize];
    public void Reset() {
        Buffer.AsSpan().Clear();
    }
}