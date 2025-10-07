namespace X3UR.Domain.Models;

public readonly struct Position(int x, int y) : IEquatable<Position> {
    public int X { get; init; } = x;
    public int Y { get; init; } = y;

    public bool Equals(Position other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is Position other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
}