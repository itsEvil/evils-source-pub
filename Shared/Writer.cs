using Shared;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Shared;
public sealed class Writer
{
    private static readonly int ByteLen = Marshal.SizeOf<byte>();
    private static readonly int ShortLen = Marshal.SizeOf<short>();
    private static readonly int IntLen = Marshal.SizeOf<int>();
    private static readonly int LongLen = Marshal.SizeOf<long>();

    public int Position;
    public void Reset()
    {
        Position = 0;
    }
    /// <summary>
    /// Writes a byte
    /// </summary>
    public void Write(Span<byte> buffer, byte value)
    {
        if (Position + ByteLen > buffer.Length)
        {
            Position++;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        buffer[Position++] = value;
    }
    /// <summary>
    /// Writes a bool
    /// </summary>
    public void Write(Span<byte> buffer, bool value) => Write(buffer, (byte)(value == true ? 1 : 0));
    /// <summary>
    /// Writes a short
    /// </summary>
    public void Write(Span<byte> buffer, short value)
    {
        if (Position + ShortLen > buffer.Length)
        {
            Position += ShortLen;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        BinaryPrimitives.WriteInt16BigEndian(buffer[Position..], value);
        Position += ShortLen;
    }
    /// <summary>
    /// Writes a ushort
    /// </summary>
    public void Write(Span<byte> buffer, ushort value)
    {
        if (Position + ShortLen > buffer.Length)
        {
            Position += ShortLen;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        BinaryPrimitives.WriteUInt16BigEndian(buffer[Position..], value);
        Position += ShortLen;
    }
    /// <summary>
    /// Writes a int
    /// </summary>
    public void Write(Span<byte> buffer, int value)
    {
        if (Position + IntLen > buffer.Length)
        {
            Position += IntLen;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        BinaryPrimitives.WriteInt32BigEndian(buffer[Position..], value);
        Position += IntLen;
    }
    /// <summary>
    /// Writes a uint
    /// </summary>
    public void Write(Span<byte> buffer, uint value)
    {
        if (Position + IntLen > buffer.Length)
        {
            Position += IntLen;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        BinaryPrimitives.WriteUInt32BigEndian(buffer[Position..], value);
        Position += IntLen;
    }
    /// <summary>
    /// Writes a long
    /// </summary>
    public void Write(Span<byte> buffer, long value)
    {
        if (Position + LongLen > buffer.Length)
        {
            Position += LongLen;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        BinaryPrimitives.WriteInt64BigEndian(buffer[Position..], value);
        Position += LongLen;
    }
    /// <summary>
    /// Writes a ulong
    /// </summary>
    public void Write(Span<byte> buffer, ulong value)
    {
        if (Position + LongLen > buffer.Length)
        {
            Position += LongLen;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        BinaryPrimitives.WriteUInt64BigEndian(buffer[Position..], value);
        Position += LongLen;
    }
    /// <summary>
    /// Writes a float
    /// </summary>
    public void Write(Span<byte> buffer, float value)
    {
        if (Position + IntLen > buffer.Length)
        {
            Position += IntLen;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        var bytes = BitConverter.GetBytes(value);
        buffer[Position] = bytes[3];
        buffer[Position + 1] = bytes[2];
        buffer[Position + 2] = bytes[1];
        buffer[Position + 3] = bytes[0];

        Position += IntLen;
    }
    /// <summary>
    /// Writes a double
    /// </summary>
    public void Write(Span<byte> buffer, double value)
    {
        if (Position + LongLen > buffer.Length)
        {
            Position += LongLen;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }


        var bytes = BitConverter.GetBytes(value);
        buffer[Position] = bytes[7];
        buffer[Position + 1] = bytes[6];
        buffer[Position + 2] = bytes[5];
        buffer[Position + 3] = bytes[4];
        buffer[Position + 4] = bytes[3];
        buffer[Position + 5] = bytes[2];
        buffer[Position + 6] = bytes[1];
        buffer[Position + 7] = bytes[0];

        Position += LongLen;
    }
    /// <summary>
    /// Writes a string using short for length of the string
    /// </summary>
    public void Write(Span<byte> buffer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Write(buffer, (short)bytes.Length);
        if (bytes.Length <= 0)
        {
#if DEBUG
            SLog.Error("String length is Zero", args: []);
#endif
            return;
        }


        if (Position + bytes.Length > buffer.Length)
        {
            Position += bytes.Length;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        bytes.CopyTo(buffer[Position..]);
        Position += bytes.Length;
    }
    /// <summary>
    /// Writes a string using int for the length of the string
    /// </summary>
    public void WriteStringInt(Span<byte> buffer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Write(buffer, bytes.Length);
        if (bytes.Length <= 0)
        {
#if DEBUG
            SLog.Error("String length is Zero", args: []);
#endif
            return;
        }


        if (Position + bytes.Length > buffer.Length)
        {
            Position += bytes.Length;
#if DEBUG
            SLog.Error("Send buffer attempted to read out of bounds {0}, {1}", args: [Position, buffer.Length]);
#endif
            return;
        }

        bytes.CopyTo(buffer[Position..]);
        Position += bytes.Length;
    }
}
