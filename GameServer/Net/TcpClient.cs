using GameServer.Net.Interfaces;
using Shared;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
namespace GameServer.Net;
public class TcpClient
{
    private readonly Socket m_Socket;
    private readonly IPAddress m_Address;
    private readonly int m_Port;

    private readonly Queue<ISend> m_SendPackets = [];
    private readonly Queue<IReceive> m_ReceivePackets = [];

    private readonly SendState m_Send = new();
    private readonly ReceiveState m_Receive = new();

    private readonly Writer m_Writer = new();
    private readonly Reader m_Reader = new();

    private const int PrefixLength = 4;

    public bool Terminate = false;

    private Task m_ReceiveThread;

    private CancellationTokenSource Source;

    public readonly string Address = "";

    //Packet structure:
    //offset = index at end of packet data
    //
    //FROM 'offset + 0' TO 'offset + 3'          = Packet Length (uint)
    //FROM 'offset + 4' TO 'offset + 5'          = Packet Id (ushort)
    //FROM 'offset + 6' TO 'offset + 6 + length' = Packet Data

    public TcpClient(IPAddress address, int port)
    {
        m_Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        m_Port = port;
        m_Address = address;
        Address = m_Address.ToString();
        Source = new();
    }
    public TcpClient(Socket socket)
    {
        m_Socket = socket;

        if (m_Socket.RemoteEndPoint is IPEndPoint endpoint)
        {
            m_Address = endpoint.Address;
            m_Port = endpoint.Port;
            Address = m_Address.ToString();
        }
        else
        {
            SLog.Debug("Failed to get address for TcpClient");
        }
        Source = new();

    }
    public void Reset()
    {
        m_SendPackets.Clear();
        m_ReceivePackets.Clear();
    }
    public void Connect()
    {
        try
        {
            m_Socket.Connect(m_Address, m_Port);
        }
        catch (Exception e)
        {
            SLog.Error(e);
        }
        SLog.Debug("Connected: {0}", args: [m_Socket.Connected]);
        BeginReceive();
    }
    public void BeginReceive()
    {
        m_ReceiveThread = Task.Run(TickReceive);
    }
    public void Tick(Client client)
    {
        client.LastMessageTime = DateTime.Now;
        while (m_ReceivePackets.TryDequeue(out var packet))
            packet.Handle(client);
    }
    public void Disconnect()
    {
        SLog.Debug("Disconnecting client");
        Source.Cancel();

        try
        {
            m_Socket.Disconnect(false);
        }
        catch (Exception e)
        {
            SLog.Error(e);
        }

        //Stopwatch watch = Stopwatch.StartNew();
        ////wait for receive to close
        //SLog.Debug("Waiting for receive thread to close");
        //while (m_ReceiveThread != null) { }
        //watch.Stop();
        //SLog.Debug("Receive thread took {0} ms", args: [watch.ElapsedMilliseconds]);
    }
    public void EnqueueSend(ISend packet)
    {
        m_SendPackets.Enqueue(packet);
    }

    public bool IsValid()
    {
        if (m_Socket == null)
            return false;

        if (!m_Socket.Connected)
            return false;

        return true;
    }
    //Ticked on worker
    public void TickReceive()
    {
        Thread.CurrentThread.IsBackground = true;
        while (!Source.IsCancellationRequested)
        {
            if (m_Socket == null)
                break;

            if (!m_Socket.Connected)
                break;

            Receive();

            Thread.Sleep(1);
        }

        m_ReceiveThread = null;
    }

    private const int MinRequiredBytes = 512;
    public void Send()
    {
        m_Writer.Reset();
        m_Send.Reset();

        int amount = 0;
        var buffer = m_Send.Data.AsSpan();
        while (m_SendPackets.TryDequeue(out var packet))
        {
#if DEBUG
            SLog.Debug("Writing packet: {0}", args: [packet.Id]);
#endif

            //Check if buffer can hold 512 bytes (around the size of the largest packet so far)
            if (m_Writer.Position + MinRequiredBytes >= buffer.Length)
                break;

            amount++;

            //Remember the idx of the length position but skip writing it for now
            var lengthPosition = m_Writer.Position;
            m_Writer.Position += 4; //sizeof uint

            //Write packet Id and packet data
            m_Writer.Write(buffer, (ushort)packet.Id);
            packet.Write(m_Writer, buffer);
            //Remember the position after packet data
            var afterPosition = m_Writer.Position;

            //Jump back so we can write the size of the packet
            m_Writer.Position = lengthPosition;

            //Calculate size and write it
            var size = afterPosition - (lengthPosition + 6); //byte length of uint + ushort = 6
            m_Writer.Write(buffer, size);
            //Jump back to after packet data
            m_Writer.Position = afterPosition;
        }

        //If we arent connected we dont try to send
        if (!m_Socket.Connected || amount == 0)
            return;

#if DEBUG
        SLog.Debug("Sending {0} packets", args: [amount]);

        var sb = new StringBuilder();
        sb.Append('[');
        for (int i = 0; i < m_Writer.Position; i++)
        {
            sb.Append(buffer[i]);
            if (i + 1 < m_Writer.Position)
                sb.Append(',');
        }
        sb.Append(']');

        SLog.Debug("Sent: {0}", args: [sb.ToString()]);
#endif


        try
        {
            m_Socket.Send(buffer[0..m_Writer.Position]);
        }
        catch (Exception e)
        {
            SLog.Error(e);
            Disconnect();
        }
    }
    public void Receive()
    {
        try
        {
            var length = m_Socket.Receive(m_Receive.Data);

            if (length == 0)
            {
                SLog.Debug("Received length is zero... likely closed connection...");
                Disconnect();
                return;
            }

            if (length < 6)
            {
                SLog.Debug("Received length less than minimum packet size of 6");
                Disconnect();
                return;
            }

            m_Reader.Reset(length);
#if DEBUG
            var sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < length; i++)
            {
                sb.Append(m_Receive.Data[i]);
                if (i + 1 < length)
                    sb.Append(',');
            }
            sb.Append(']');
            SLog.Debug("Received: {0}", args: [sb.ToString()]);
#endif
            Read(length);
            m_Receive.Reset();
        }
        catch (Exception e)
        {
            SLog.Error(e);
            Disconnect();
        }
    }
    private void Read(int totalLength)
    {
        var buffer = m_Receive.Data.AsSpan();

        while (m_Reader.Position + 6 < totalLength)
        {
            uint packetLength = m_Reader.UInt(buffer);
            var packetId = m_Reader.UShort(buffer);

            if (packetLength + m_Reader.Position > totalLength)
            {
                SLog.Error("Total length is smaller then packet length + position. {0}, {1}, {2}", args: [packetLength, m_Reader.Position - 6, totalLength]);
                return;
            }

            if (!PacketHandler.TryGetPacket(packetId, m_Reader, m_Receive.Data, out var packet))
            {
                SLog.Error("Failed to find packet with id {0}", args: [packetId]);
                continue;
            }

            m_ReceivePackets.Enqueue(packet);
            SLog.Debug("Receive id:{0} length:{1} total:{2}", args: [packetId, packetLength, totalLength]);
        }
    }
}
