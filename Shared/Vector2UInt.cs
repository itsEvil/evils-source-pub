using System.Numerics;

namespace Shared;
public struct Vector2UInt
{
    public uint X;
    public uint Y;

    public Vector2UInt(uint x, uint y)
    {
        X = x;
        Y = y;
    }
    public Vector2UInt(Vector2 position)
    {
        X = (uint)position.X;
        Y = (uint)position.Y;
    }

    public override readonly string ToString()
    {
        return $"X: {X} Y:{Y}";
    }
    //public static Vector2UInt operator +(Vector2UInt a, Vector2Int b) {
    //    return new Vector2UInt(a.X + b.X, a.Y + b.Y);
    //}
    //public static Vector2UInt operator -(Vector2UInt a, Vector2Int b) {
    //    return new Vector2UInt(a.X - b.X, a.Y - b.Y);
    //}
    //public static Vector2UInt operator /(Vector2UInt a, Vector2Int b) {
    //    return new Vector2UInt(a.X / b.X, a.Y / b.Y);
    //}
    //public static Vector2UInt operator *(Vector2UInt a, Vector2Int b) {
    //    return new Vector2UInt(a.X * b.X, a.Y * b.Y);
    //}
    //public static Vector2UInt operator +(Vector2UInt a, Vector2 b) {
    //    return new Vector2UInt((int)(a.X + b.X),(int)(a.Y + b.Y));
    //}
    //public static Vector2UInt operator -(Vector2UInt a, Vector2 b) {
    //    return new Vector2UInt((int)(a.X - b.X), (int)(a.Y - b.Y));
    //}
    //public static Vector2UInt operator /(Vector2UInt a, Vector2 b) {
    //    return new Vector2UInt((int)(a.X / b.X), (int)(a.Y / b.Y));
    //}
    //public static Vector2UInt operator *(Vector2UInt a, Vector2 b) {
    //    return new Vector2UInt((int)(a.X * b.X), (int)(a.Y * b.Y));
    //}
}
