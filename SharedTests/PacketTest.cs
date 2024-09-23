using Shared;

namespace SharedTests;
[TestClass]
public class PacketTest
{
    private byte[] Buffer = new byte[1024];

    [TestInitialize]
    public void Init()
    {
        Buffer.AsSpan().Clear();
    }

    [TestMethod("Write")]
    public void Write() {
        var w = new Writer();
        Assert.AreEqual(0, w.Position);
        Assert.AreEqual(1024, Buffer.Length);
        var b = Buffer.AsSpan();

        w.Write(b, false);
        w.Write(b, true);

        Assert.AreEqual(2, w.Position);

        w.Write(b, (short)0);
        w.Write(b, (short)15);
        w.Write(b, (short)short.MaxValue);

        Assert.AreEqual(8, w.Position);
        
        w.Write(b, (ushort)0);
        w.Write(b, (ushort)15);
        w.Write(b, (ushort)ushort.MaxValue);
        
        Assert.AreEqual(14, w.Position);

        w.Write(b, (int)0);
        w.Write(b, (int)15);
        w.Write(b, (int)int.MaxValue);

        Assert.AreEqual(26, w.Position);

        w.Write(b, (uint)0);
        w.Write(b, (uint)15);
        w.Write(b, (uint)uint.MaxValue);

        Assert.AreEqual(38, w.Position);

        w.Write(b, (long)0);
        w.Write(b, (long)15);
        w.Write(b, (long)long.MaxValue);

        Assert.AreEqual(62, w.Position);

        w.Write(b, (ulong)0);
        w.Write(b, (ulong)15);
        w.Write(b, (ulong)ulong.MaxValue);

        Assert.AreEqual(86, w.Position);
        
        //ushort lenght + string length
        w.Write(b, "Hello world");
        Assert.AreEqual(99, w.Position);

        w.WriteStringInt(b, "Hello world");
        w.WriteStringInt(b, "");
        Assert.AreEqual(118, w.Position);
    }

    [TestMethod("Read")]
    public void Read() {
        Write();

        var b = Buffer.AsSpan();
        var r = new Reader();
        r.Reset(b.Length);

        var b1 = r.Bool(b);
        var b2 = r.Bool(b);

        Assert.AreEqual(false, b1);
        Assert.AreEqual(true, b2);

        var short1 = r.Short(b);
        var short2 = r.Short(b);
        var short3 = r.Short(b);

        Assert.AreEqual(0, short1);
        Assert.AreEqual(15, short2);
        Assert.AreEqual(short.MaxValue, short3);

        var ushort1 = r.UShort(b);
        var ushort2 = r.UShort(b);
        var ushort3 = r.UShort(b);

        Assert.AreEqual(0, ushort1);
        Assert.AreEqual(15, ushort2);
        Assert.AreEqual(ushort.MaxValue, ushort3);

        var int1 = r.Int(b);
        var int2 = r.Int(b);
        var int3 = r.Int(b);

        Assert.AreEqual(0, int1);
        Assert.AreEqual(15, int2);
        Assert.AreEqual(int.MaxValue, int3);

        var uint1 = r.UInt(b);
        var uint2 = r.UInt(b);
        var uint3 = r.UInt(b);

        Assert.AreEqual<uint>(0, uint1);
        Assert.AreEqual<uint>(15, uint2);
        Assert.AreEqual(uint.MaxValue, uint3);

        var long1 = r.Long(b);
        var long2 = r.Long(b);
        var long3 = r.Long(b);

        Assert.AreEqual(0, long1);
        Assert.AreEqual(15, long2);
        Assert.AreEqual(long.MaxValue, long3);

        var ulong1 = r.ULong(b);
        var ulong2 = r.ULong(b);
        var ulong3 = r.ULong(b);

        Assert.AreEqual<ulong>(0, ulong1);
        Assert.AreEqual<ulong>(15, ulong2);
        Assert.AreEqual<ulong>(ulong.MaxValue, ulong3);


        var string1 = r.StringShort(b);
        var string2 = r.StringInt(b);
        var string3 = r.StringInt(b);

        Assert.AreEqual("Hello world", string1);
        Assert.AreEqual("Hello world", string2);
        Assert.AreEqual("", string3);

        Assert.AreEqual(118, r.Position);
    }
}