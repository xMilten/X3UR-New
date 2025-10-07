using X3UR.Domain.DTOs;

namespace X3UR.Domain.Models;

public class Cluster {
    private List<Sector> _growableSectors;
    private List<Cluster> _neighbors;

    public byte X { get; init; }
    public byte Y { get; init; }
    public RaceSettingDto Race { get; init; }
    public List<Sector> Sectors { get; } = new();

    public void AddGrowableSector(Sector sector) {
        _growableSectors ??= new List<Sector>();
        _growableSectors.Add(sector);
    }

    public void RemoveGrowableSector(Sector sector) {
        _growableSectors?.Remove(sector);
    }

    public void AddNeighbor(Cluster cluster) {
        _neighbors ??= new List<Cluster>();
        _neighbors.Add(cluster);
    }

    public void RemoveNeighbor(Cluster cluster) {
        _neighbors?.Remove(cluster);
    }

    public bool CanGrow() => (_growableSectors != null && _growableSectors.Count > 0) &&
                             (_growableSectors.Count < Race.MaxClusterSize);
    public bool HasNeighbors() => _neighbors != null && _neighbors.Count > 0;

    public void SortNeighborsDesc() {
        var arr = new (Cluster neighbor, int dist)[_neighbors.Count];
        for (int i = 0; i < _neighbors.Count; i++) {
            var n = _neighbors[i];
            int dx = n.X - X;
            int dy = n.Y - Y;
            arr[i] = (n, dx * dx + dy * dy);
        }

        Array.Sort(arr, (t1, t2) => t2.dist.CompareTo(t1.dist));

        for (int i = 0; i < arr.Length; i++)
            _neighbors[i] = arr[i].neighbor;
    }
}