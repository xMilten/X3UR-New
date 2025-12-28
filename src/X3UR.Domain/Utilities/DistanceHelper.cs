using X3UR.Domain.Models;

namespace X3UR.Domain.Utilities;

public class DistanceHelper {
    public static int DistanceSquared(int x1, int y1, int x2, int y2) {
        int dx = x1 - x2;
        int dy = y1 - y2;
        return dx * dx + dy * dy;
    }

    public static int DistanceSquared((int X, int Y) a, (int X, int Y) b)
        => DistanceSquared(a.X, a.Y, b.X, b.Y);

    public static int DistanceSquared(Sector a, Sector b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return DistanceSquared(a.X, a.Y, b.X, b.Y);
    }

    public static int DistanceSquared(Cluster a, Cluster b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return DistanceSquared(a.X, a.Y, b.X, b.Y);
    }
}