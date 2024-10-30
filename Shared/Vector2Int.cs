using System.Numerics;

namespace Shared;
public struct Vector2Int(int x, int y) {
    public int X = x;
    public int Y = y;
    public static Vector2Int operator +(Vector2Int a, Vector2Int b) {
        return new Vector2Int(a.X + b.X, a.Y + b.Y);
    }
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) {
        return new Vector2Int(a.X - b.X, a.Y - b.Y);
    }
    public static Vector2Int operator /(Vector2Int a, Vector2Int b) {
        return new Vector2Int(a.X / b.X, a.Y / b.Y);
    }
    public static Vector2Int operator *(Vector2Int a, Vector2Int b) {
        return new Vector2Int(a.X * b.X, a.Y * b.Y);
    }
    public static Vector2Int operator +(Vector2Int a, Vector2 b) {
        return new Vector2Int((int)(a.X + b.X),(int)(a.Y + b.Y));
    }
    public static Vector2Int operator -(Vector2Int a, Vector2 b) {
        return new Vector2Int((int)(a.X - b.X), (int)(a.Y - b.Y));
    }
    public static Vector2Int operator /(Vector2Int a, Vector2 b) {
        return new Vector2Int((int)(a.X / b.X), (int)(a.Y / b.Y));
    }
    public static Vector2Int operator *(Vector2Int a, Vector2 b) {
        return new Vector2Int((int)(a.X * b.X), (int)(a.Y * b.Y));
    }
}
