using System;
using System.Buffers.Binary;
using System.Text;

namespace game_server.Networking;
public static class PacketUtils
{
    public static void WriteByte(Span<byte> buffer, byte value, ref int ptr)
    {
        if (ptr + 1 > buffer.Length)
        {
            ptr++;
            throw new Exception($"Receive buffer attempted to write out of bounds {ptr}, {buffer.Length}");
        }

        buffer[ptr++] = value;
    }
    public static void WriteBool(Span<byte> buffer, bool value, ref int ptr)
    {
        //SLog.Log("Trying to write bool {0} at {1}", value, ptr);
        WriteByte(buffer, value == true ? (byte)1 : (byte)0, ref ptr);
    }
    public static void WriteShort(Span<byte> buffer, short value, ref int ptr)
    {
        //SLog.Log("Trying to write short {0} at {1}", value, ptr);
        if (ptr + 2 > buffer.Length)
        {
            ptr += 2;
            throw new Exception($"Receive buffer attempted to write out of bounds {ptr}, {buffer.Length}");
        }

        BinaryPrimitives.WriteInt16BigEndian(buffer[ptr..], value);
        ptr += 2;
    }
    public static void WriteUShort(Span<byte> buffer, ushort value, ref int ptr)
    {
        //SLog.Log("Trying to write ushort {0} at {1}", value, ptr);
        if (ptr + 2 > buffer.Length)
        {
            ptr += 2;
            throw new Exception($"Receive buffer attempted to write out of bounds {ptr}, {buffer.Length}");
        }

        BinaryPrimitives.WriteUInt16BigEndian(buffer[ptr..], value);
        ptr += 2;
    }
    public static void WriteInt(Span<byte> buffer, int value, ref int ptr)
    {
        //SLog.Log("Trying to write int {0}, {1}", value, ptr);
        if (ptr + 4 > buffer.Length)
        {
            ptr += 4;
            throw new Exception($"Receive buffer attempted to write out of bounds {ptr}, {buffer.Length}");
        }

        BinaryPrimitives.WriteInt32BigEndian(buffer[ptr..], value);
        ptr += 4;
    }
    public static void WriteFloat(Span<byte> buffer, float value, ref int ptr)
    {
        if (ptr + 4 > buffer.Length)
        {
            ptr += 4;
            throw new Exception($"Receive buffer attempted to write out of bounds {ptr}, {buffer.Length}");
        }

        var bytes = BitConverter.GetBytes(value);
        buffer[ptr] = bytes[3];
        buffer[ptr + 1] = bytes[2];
        buffer[ptr + 2] = bytes[1];
        buffer[ptr + 3] = bytes[0];
        ptr += 4;
    }
    public static void WriteUInt(Span<byte> buffer, uint value, ref int ptr)
    {
        //SLog.Log("Trying to write uint {0} at {1}", value, ptr);
        if (ptr + 4 > buffer.Length)
        {
            ptr += 4;
            throw new Exception($"Receive buffer attempted to write out of bounds {ptr}, {buffer.Length}");
        }

        BinaryPrimitives.WriteUInt32BigEndian(buffer[ptr..], value);
        ptr += 4;
    }
    public static void WriteLong(Span<byte> buffer, long value, ref int ptr)
    {
        //SLog.Log("Trying to write long {0} at {1}", value, ptr);
        if (ptr + 8 > buffer.Length)
        {
            ptr += 8;
            throw new Exception($"Receive buffer attempted to write out of bounds {ptr}, {buffer.Length}");
        }

        BinaryPrimitives.WriteInt64BigEndian(buffer[ptr..], value);
        ptr += 8;
    }
    public static void WriteULong(Span<byte> buffer, ulong value, ref int ptr)
    {
        //SLog.Log("Trying to write ulong {0} at {1}", value, ptr);
        if (ptr + 8 > buffer.Length)
        {
            ptr += 8;
            throw new Exception($"Receive buffer attempted to write out of bounds {ptr}, {buffer.Length}");
        }

        BinaryPrimitives.WriteUInt64BigEndian(buffer[ptr..], value);
        ptr += 8;
    }
    public static void WriteString(Span<byte> buffer, string value, ref int ptr)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        if (ptr + 2 + (ushort)bytes.Length > buffer.Length)
        {
            ptr += 2 + bytes.Length;
            throw new Exception($"Receive buffer attempted to write out of bounds {ptr} {buffer.Length}");
        }

        WriteUShort(buffer, (ushort)bytes.Length, ref ptr);
        bytes.CopyTo(buffer[ptr..]);
        ptr += bytes.Length;
    }
    public static byte ReadByte(Span<byte> buffer, ref int ptr, int len)
    {
        if (ptr + 1 > len)
        {
            ptr++;
            throw new Exception($"Receive buffer attempted to read out of bounds {ptr}, {len}");
        }

        var data = buffer[ptr];
        ptr++;
        return data;
    }
    public static bool ReadBool(Span<byte> buffer, ref int ptr, int len)
    {
        //SLog.Log("Trying to read bool at {0}", ptr);
        var data = ReadByte(buffer, ref ptr, len);
        return data == 1;
    }
    public static short ReadShort(Span<byte> buffer, ref int ptr, int len)
    {
        //SLog.Log("Trying to read short at {0}", ptr);

        if (ptr + 2 > len)
        {
            ptr += 2;
            throw new Exception($"Receive buffer attempted to read out of bounds {ptr}, {len}");
        }

        var data = BinaryPrimitives.ReadInt16BigEndian(buffer[ptr..]);
        ptr += 2;
        return data;
    }
    public static ushort ReadUShort(Span<byte> buffer, ref int ptr, int len)
    {
        //SLog.Log("Trying to read ushort at {0}", ptr);

        if (ptr + 2 > len)
        {
            ptr += 2;
            throw new Exception($"Receive buffer attempted to read out of bounds {ptr}, {len}");
        }

        var data = BinaryPrimitives.ReadUInt16BigEndian(buffer[ptr..]);
        ptr += 2;
        return data;
    }
    public static int ReadInt(Span<byte> buffer, ref int ptr, int len)
    {
        //SLog.Log("Trying to read int at {0}, {1}", ptr, len);

        if (ptr + 4 > len)
        {
            ptr += 4;
            throw new Exception($"Receive buffer attempted to read out of bounds {ptr}, {len}");
        }

        var data = BinaryPrimitives.ReadInt32BigEndian(buffer[ptr..]);
        ptr += 4;
        return data;
    }
    public static uint ReadUInt(Span<byte> buffer, ref int ptr, int len)
    {
        //SLog.Log("Trying to read uint at {0}", ptr);

        if (ptr + 4 > len)
        {
            ptr += 4;
            throw new Exception($"Receive buffer attempted to read out of bounds {ptr}, {len}");
        }

        var data = BinaryPrimitives.ReadUInt32BigEndian(buffer[ptr..]);
        ptr += 4;
        return data;
    }
    public static float ReadFloat(Span<byte> buffer, ref int ptr, int len)
    {
        //SLog.Log("Trying to read float at {0}, {1}", ptr, len);  
        if (ptr + 4 > len)
        {
            ptr += 4;
            throw new Exception($"Receive buffer attempted to read out of bounds {ptr}, {len}");
        }

        Span<byte> buf = new byte[4];
        buf[3] = buffer[ptr];
        buf[2] = buffer[ptr + 1];
        buf[1] = buffer[ptr + 2];
        buf[0] = buffer[ptr + 3];


        float data = BitConverter.ToSingle(buf);
        ptr += 4;
        return data;
    }
    public static long ReadLong(Span<byte> buffer, ref int ptr, int len)
    {
        //SLog.Log("Trying to read long at {0}", ptr);

        if (ptr + 8 > len)
        {
            ptr += 8;
            throw new Exception($"Receive buffer attempted to read out of bounds {ptr}, {len}");
        }

        var data = BinaryPrimitives.ReadInt64BigEndian(buffer[ptr..]);
        ptr += 8;
        return data;
    }
    public static ulong ReadULong(Span<byte> buffer, ref int ptr, int len)
    {
        //SLog.Log("Trying to read ulong at {0}", ptr);

        if (ptr + 8 > len)
        {
            ptr += 8;
            throw new Exception($"Receive buffer attempted to read out of bounds {ptr}, {len}");
        }

        var data = BinaryPrimitives.ReadUInt64BigEndian(buffer[ptr..]);
        ptr += 8;
        return data;
    }
    public static string ReadString(Span<byte> buffer, ref int ptr, int len)
    {
        short strLen = ReadShort(buffer, ref ptr, len);
        if (strLen <= 0)
            return "";

        if (ptr + strLen > len)
        {
            ptr += strLen;
            throw new Exception($"Receive buffer attempted to read out of bounds {ptr}, {len}");
        }

        var data = Encoding.UTF8.GetString(buffer[ptr..(ptr + strLen)]);
        ptr += strLen;

        //Utils.Log("Read string '{0}'", data);
        return data;
    }
}