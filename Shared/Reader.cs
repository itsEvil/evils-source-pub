﻿using Shared;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Shared;
public sealed class Reader
{
    private static readonly int ByteLen = Marshal.SizeOf<byte>();
    private static readonly int ShortLen = Marshal.SizeOf<short>();
    private static readonly int IntLen = Marshal.SizeOf<int>();
    private static readonly int LongLen = Marshal.SizeOf<long>();

    public int Length;
    public int Position;

    public void Reset(int length)
    {
        Length = length;
        Position = 0;
    }
    public byte Byte(Span<byte> buffer)
    {
        if (Position + ByteLen > Length)
        {
            Position++;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return 0;
        }

        return buffer[Position++];
    }
    public bool Bool(Span<byte> buffer) => Byte(buffer) == 1;
    public short Short(Span<byte> buffer)
    {
        if (Position + ShortLen > Length)
        {
            Position += ShortLen;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return 0;
        }

        var data = BinaryPrimitives.ReadInt16BigEndian(buffer[Position..]);
        Position += ShortLen;
        return data;
    }
    public ushort UShort(Span<byte> buffer)
    {
        if (Position + ShortLen > Length)
        {
            Position += ShortLen;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return 0;
        }

        var data = BinaryPrimitives.ReadUInt16BigEndian(buffer[Position..]);
        Position += ShortLen;
        return data;
    }
    public int Int(Span<byte> buffer)
    {
        if (Position + IntLen > Length)
        {
            Position += IntLen;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return 0;
        }

        var data = BinaryPrimitives.ReadInt32BigEndian(buffer[Position..]);
        Position += IntLen;
        return data;
    }
    public uint UInt(Span<byte> buffer)
    {
        if (Position + IntLen > Length)
        {
            Position += IntLen;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return 0;
        }

        var data = BinaryPrimitives.ReadUInt32BigEndian(buffer[Position..]);
        Position += IntLen;
        return data;
    }

    public long Long(Span<byte> buffer)
    {
        if (Position + LongLen > Length)
        {
            Position += LongLen;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return 0;
        }

        var data = BinaryPrimitives.ReadInt64BigEndian(buffer[Position..]);
        Position += LongLen;
        return data;
    }

    public ulong ULong(Span<byte> buffer)
    {
        if (Position + LongLen > Length)
        {
            Position += LongLen;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return 0;
        }

        var data = BinaryPrimitives.ReadUInt64BigEndian(buffer[Position..]);
        Position += LongLen;
        return data;
    }

    public float Float(Span<byte> buffer)
    {
        if (Position + IntLen > Length)
        {
            Position += IntLen;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return 0;
        }

        Span<byte> buf = stackalloc byte[4];
        buf[3] = buffer[Position];
        buf[2] = buffer[Position + 1];
        buf[1] = buffer[Position + 2];
        buf[0] = buffer[Position + 3];


        var data = BitConverter.ToSingle(buf);
        Position += IntLen;
        return data;
    }
    public double Double(Span<byte> buffer)
    {
        if (Position + LongLen > Length)
        {
            Position += LongLen;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return 0;
        }

        Span<byte> buf = stackalloc byte[8];
        buf[7] = buffer[Position];
        buf[6] = buffer[Position + 1];
        buf[5] = buffer[Position + 2];
        buf[4] = buffer[Position + 3];
        buf[3] = buffer[Position + 4];
        buf[2] = buffer[Position + 5];
        buf[1] = buffer[Position + 6];
        buf[0] = buffer[Position + 7];

        var data = BitConverter.ToDouble(buf);
        Position += LongLen;
        return data;
    }
    /// <summary>
    /// Reads a <see cref="short"/> length then tries to read <see cref="string"/> using the length.
    /// </summary>
    public string StringShort(Span<byte> buffer)
    {
        var length = Short(buffer);
        if (length <= 0)
        {
#if DEBUG
            SLog.Error("String length is Zero", args: []);
#endif
            return "";
        }


        if (Position + length > Length)
        {
            Position += length;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return "";
        }

        var data = Encoding.UTF8.GetString(buffer[Position..(Position + length)]);
        Position += length;

        SLog.Debug("Read String-{1}: {0}", args: [data, length]);

        return data;
    }
    /// <summary>
    /// Reads a <see cref="int"/> length then tries to read <see cref="string"/> using the length.
    /// </summary>
    public string StringInt(Span<byte> buffer)
    {
        var length = Int(buffer);
        if (length <= 0)
        {
#if DEBUG
            SLog.Error("String length is Zero", args: []);
#endif
            return "";
        }


        if (Position + length > Length)
        {
            Position += length;
#if DEBUG
            SLog.Error("Receive buffer attempted to read out of bounds {0}, {1}", args: [Position, Length]);
#endif
            return "";
        }

        var data = Encoding.UTF8.GetString(buffer[Position..(Position + length)]);
        Position += length;

        SLog.Debug("Read String-{1}: {0}", args: [data, length]);

        return data;
    }

    /// <summary>
    /// Only primatives are supported by this method, try not to use this as it will allocate objects when reading the primative
    /// </summary>
    public T[] ReadArray<T>(Span<byte> b)
    {
        var len = UShort(b);

        T[] arr = new T[len];

        for (int i = 0; i < arr.Length; i++)
            arr[i] = Read<T>(b);

        return arr;
    }
    /// <summary>
    /// Read a primative of type T, try not to use this as it needs to cast to object then T when reading the primative
    /// </summary>
    public T Read<T>(Span<byte> b, T ignoreThisParameter = default)
    {
        switch (ignoreThisParameter)
        {
            case byte: return (T)(object)Byte(b);
            case bool: return (T)(object)Bool(b); 
            case short: return (T)(object)Short(b);
            case ushort: return (T)(object)UShort(b);
            case int: return (T)(object)Int(b);
            case uint: return (T)(object)UInt(b);
            case float: return (T)(object)Float(b);
            case double: return (T)(object)Double(b);
            case long: return (T)(object)Long(b);
            case ulong: return (T)(object)ULong(b);
            case string: {
                    var len = Int(b);

                    //If the length of the string is less than 65,535
                    if (len < ushort.MaxValue) {
                        Position -= 4;
                        return (T)(object)StringShort(b);
                    }
                    else return (T)(object)StringInt(b);
                }
            default: throw new Exception($"Type {ignoreThisParameter.GetType()} not supported by Writer");
        }
    }
}
