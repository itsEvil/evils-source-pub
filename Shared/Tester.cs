namespace Shared;
public static class Tester {
    public static bool Assert(bool condition) {
        if (!condition)
            throw new Exception($"Condition: {condition} is false");
        
        return true;
    }
    public static bool Assert(byte[] a, byte[] b)
        => Assert(a.SequenceEqual(b));
    public static bool Assert(uint a, uint b)
        => Assert(a == b);
    public static bool Assert(int a, int b)
        => Assert(a == b);
    public static bool Assert(object a, object b)
        => Assert(ReferenceEquals(a,b));
}
