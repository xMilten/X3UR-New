using System.Collections.Concurrent;

namespace X3UR.Domain.Utilities;

internal static class ListPool<T> {
    private static readonly ConcurrentBag<List<T>> _pool = new();

    public static List<T> Rent() {
        if (_pool.TryTake(out var list)) {
            list.Clear();
            return list;
        }
        return new List<T>();
    }

    /// <summary>
    /// Returns a list to the object pool after clearing its contents.
    /// </summary>
    /// <remarks>This method clears the contents of the provided list before adding it back to the object
    /// pool.  Ensure that the list is no longer in use before calling this method to avoid unintended side
    /// effects.</remarks>
    /// <param name="list">The list to be returned to the pool. If <paramref name="list"/> is <see langword="null"/>, the method does
    /// nothing.</param>
    public static void Return(List<T> list) {
        if (list == null)
            return;
        list.Clear();
        _pool.Add(list);
    }
}