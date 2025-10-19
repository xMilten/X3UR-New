using X3UR.Domain.DTOs;
using X3UR.Domain.Interfaces;

namespace X3UR.Domain.Models;

public class Cluster {
    private List<Sector> _sectors;
    private List<Sector> _growableSectors;
    private List<Cluster> _neighbors;

    public byte X { get; init; }
    public byte Y { get; init; }
    public RaceSettingDto Race { get; init; }

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

    /// <summary>
    /// Ob der Cluster noch wachsen kann.
    /// (Ob es noch Sektoren hat die wachsen können und ob die maximale Clustergröße noch nicht erreicht ist)
    /// </summary>
    /// <returns></returns>
    public bool CanGrow() => _growableSectors != null
                          && _growableSectors.Count > 0
                          && _growableSectors.Count < Race.MaxClusterSize;

    public Sector GetRandomGrowableSector(IRandomProvider rand) {
        if (!CanGrow()) return null;

        int index = rand.Next(0, _growableSectors.Count);
        return _growableSectors[index];
    }

    /// <summary>
    /// Gib den wachsenden Sektor, der am nächsten zum Nachbar-Cluster dieses Clusters liegt
    /// </summary>
    /// <returns></returns>
    public Sector GetBestGrowableSectorTowardsNeighbor() {
        Cluster closestNeighborCluster = GetClosestNeighborCluster();

        if (!CanGrow() || closestNeighborCluster == null) return null;

        Sector bestSector = null;
        int bestDist = int.MaxValue;

        foreach (var sector in _growableSectors) {
            int dx = closestNeighborCluster.X - sector.X;
            int dy = closestNeighborCluster.Y - sector.Y;
            int dist = dx * dx + dy * dy; // Quadrat der Distanz
            if (dist < bestDist) {
                bestDist = dist;
                bestSector = sector;
            }
        }
        return bestSector;
    }

    public bool HasNeighbors() => _neighbors != null && _neighbors.Count > 0;

    public int GetNeighborCount() => _neighbors?.Count ?? 0;

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

    public Cluster GetClosestNeighborCluster() {
        return _neighbors[^1];
    }

    /// <summary>
    /// Fügt einen Sektor zum Cluster hinzu und falls der Sektor wachsen kann, auch zur Liste der wachsenden Sektoren.
    /// </summary>
    /// <param name="sector"></param>
    public void AddSector(Sector sector) {
        _sectors ??= new List<Sector>();
        _sectors.Add(sector);
        if (sector.CanGrow())
            AddGrowableSector(sector);
    }
}