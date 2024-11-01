using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
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
    public void Write(Span<byte> buffer, byte value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        if (Position + ByteLen > buffer.Length)
        {
            Position++;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }

        buffer[Position++] = value;
    }
    /// <summary>
    /// Writes a bool
    /// </summary>
    public void Write(Span<byte> buffer, bool value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0) => Write(buffer, (byte)(value == true ? 1 : 0), caller, path, line);
    /// <summary>
    /// Writes a short
    /// </summary>
    public void Write(Span<byte> buffer, short value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        if (Position + ShortLen > buffer.Length)
        {
            Position += ShortLen;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }

        BinaryPrimitives.WriteInt16BigEndian(buffer[Position..], value);
        Position += ShortLen;
    }
    /// <summary>
    /// Writes a ushort
    /// </summary>
    public void Write(Span<byte> buffer, ushort value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        if (Position + ShortLen > buffer.Length)
        {
            Position += ShortLen;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }

        BinaryPrimitives.WriteUInt16BigEndian(buffer[Position..], value);
        Position += ShortLen;
    }
    /// <summary>
    /// Writes a int
    /// </summary>
    public void Write(Span<byte> buffer, int value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        if (Position + IntLen > buffer.Length)
        {
            Position += IntLen;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }

        BinaryPrimitives.WriteInt32BigEndian(buffer[Position..], value);
        Position += IntLen;
    }
    /// <summary>
    /// Writes a uint
    /// </summary>
    public void Write(Span<byte> buffer, uint value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        if (Position + IntLen > buffer.Length)
        {
            Position += IntLen;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }

        BinaryPrimitives.WriteUInt32BigEndian(buffer[Position..], value);
        Position += IntLen;
    }
    /// <summary>
    /// Writes a long
    /// </summary>
    public void Write(Span<byte> buffer, long value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        if (Position + LongLen > buffer.Length)
        {
            Position += LongLen;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }

        BinaryPrimitives.WriteInt64BigEndian(buffer[Position..], value);
        Position += LongLen;
    }
    /// <summary>
    /// Writes a ulong
    /// </summary>
    public void Write(Span<byte> buffer, ulong value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        if (Position + LongLen > buffer.Length)
        {
            Position += LongLen;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }

        BinaryPrimitives.WriteUInt64BigEndian(buffer[Position..], value);
        Position += LongLen;
    }
    /// <summary>
    /// Writes a float
    /// </summary>
    public void Write(Span<byte> buffer, float value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        if (Position + IntLen > buffer.Length)
        {
            Position += IntLen;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
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
    public void Write(Span<byte> buffer, double value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        if (Position + LongLen > buffer.Length)
        {
            Position += LongLen;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
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
    public void Write(Span<byte> buffer, string value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Write(buffer, (ushort)bytes.Length);
        if (bytes.Length <= 0)
        {
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }


        if (Position + bytes.Length > buffer.Length)
        {
            Position += bytes.Length;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }

        bytes.CopyTo(buffer[Position..]);
        Position += bytes.Length;
    }
    /// <summary>
    /// Writes a string using int for the length of the string
    /// </summary>
    public void WriteStringInt(Span<byte> buffer, string value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        Write(buffer, bytes.Length);
        if (bytes.Length <= 0)
        {
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }


        if (Position + bytes.Length > buffer.Length)
        {
            Position += bytes.Length;
            var ex = new Exception($"Writer attempted to write out of bounds: {Position}, {buffer.Length} via {caller} \n\t at {path} L.{line}");
#if DEBUG
            SLog.Error(ex);
#endif
            throw ex;
        }

        bytes.CopyTo(buffer[Position..]);
        Position += bytes.Length;
    }

    /// <summary>
    /// Only primatives are supported by this method.
    /// </summary>
    public void WriteArray<T>(Span<byte> buffer, T[] array, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        Write(buffer, (ushort)array.Length);
        var span = array.AsSpan();
        for (int i = 0; i < array.Length; i++)
            Write(buffer, span[i]);
    }
    /// <summary>
    /// Write a primative of type T
    /// </summary>
    public void Write<T>(Span<byte> b, T value, [CallerMemberName] string caller = "", [CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
    {
        switch (value)
        {
            case byte v: Write(b, v, caller, path, line); break;
            case bool v: Write(b, v, caller, path, line); break;
            case short v: Write(b, v, caller, path, line); break;
            case ushort v: Write(b, v, caller, path, line); break;
            case int v: Write(b, v, caller, path, line); break;
            case uint v: Write(b, v, caller, path, line); break;
            case float v: Write(b, v, caller, path, line); break;
            case double v: Write(b, v, caller, path, line); break;
            case long v: Write(b, v, caller, path, line); break;
            case ulong v: Write(b, v, caller, path, line); break;
            case string v:
                {
                    if (v.Length >= ushort.MaxValue)
                        WriteStringInt(b, v, caller, path, line);
                    else
                        Write(b, v, caller, path, line);
                }
                break;
            default: throw new Exception($"Type {value.GetType()} not supported by Writer via {caller} \n\t at {path} L.{line}");
        }
    }
}
