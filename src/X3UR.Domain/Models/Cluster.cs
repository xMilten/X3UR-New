using X3UR.Domain.DTOs;
using X3UR.Domain.Interfaces;
using X3UR.Domain.Utilities;

namespace X3UR.Domain.Models;

public class Cluster {
    private List<Sector> _sectors;
    private List<Sector> _growableSectors;
    private List<Cluster> _neighbors;

    public byte X { get; init; }
    public byte Y { get; init; }
    public RaceSettingDto Race { get; init; }

    public event Action<Sector>? SectorAdded;

    public Cluster() {
        _sectors = new List<Sector>();
        _growableSectors = null;
        _neighbors = null;
    }

    public IReadOnlyList<Sector> GetSectors() => _sectors.AsReadOnly();

    public void AddGrowableSector(Sector sector) {
        ArgumentNullException.ThrowIfNull(sector);
        _growableSectors ??= ListPool<Sector>.Rent();

        if (!_growableSectors.Contains(sector))
            _growableSectors.Add(sector);
    }

    public void RemoveGrowableSector(Sector sector) {
        _growableSectors?.Remove(sector);

        if (_growableSectors != null && _growableSectors.Count == 0) {
            ListPool<Sector>.Return(_growableSectors);
            _growableSectors = null;
        }
    }

    public void AddNeighbor(Cluster cluster) {
        ArgumentNullException.ThrowIfNull(cluster);
        _neighbors ??= new List<Cluster>();

        if (!_neighbors.Contains(cluster))
            _neighbors.Add(cluster);
    }

    public void RemoveNeighbor(Cluster cluster) {
        _neighbors?.Remove(cluster);
    }

    /// <summary>
    /// Get whether the cluster can grow (has growable sectors and is below max size).
    /// </summary>
    public bool CanGrow() => _growableSectors != null
                          && _growableSectors.Count > 0
                          && _sectors.Count < Race.MaxClusterSize;

    public Sector GetRandomGrowableSector(IRandomProvider rnd) {
        if (!CanGrow())
            throw new InvalidOperationException("Cluster cannot grow.");

        int index = rnd.Next(0, _growableSectors.Count);
        return _growableSectors[index];
    }

    /// <summary>
    /// Get the growable sector that is closest to the nearest neighbor cluster.
    /// </summary>
    public Sector GetGrowableSectorTowardsNeighbor() {
        if (!CanGrow() || !HasNeighbors())
            throw new InvalidOperationException("Cluster cannot grow or has no neighbors.");

        Cluster closestNeighborCluster = GetClosestNeighborCluster();
        Sector closestGrowableSector = null;
        int bestDist = int.MaxValue;

        foreach (var growableSector in _growableSectors) {
            int dist = DistanceHelper.DistanceSquared(closestNeighborCluster.X, closestNeighborCluster.Y, growableSector.X, growableSector.Y);

            if (dist < bestDist) {
                bestDist = dist;
                closestGrowableSector = growableSector;
            }
        }
        return closestGrowableSector;
    }

    public bool HasNeighbors() => _neighbors != null && _neighbors.Count > 0;

    public int GetNeighborCount() => _neighbors?.Count ?? 0;

    public void SortNeighborsDesc() {
        if (_neighbors == null || _neighbors.Count == 0) return;
        var arr = new (Cluster neighbor, int dist)[_neighbors.Count];
        _neighbors.Sort((a, b) => {
            int da = (a.X - X) * (a.X - X) + (a.Y - Y) * (a.Y - Y);
            int db = (b.X - X) * (b.X - X) + (b.Y - Y) * (b.Y - Y);
            return db.CompareTo(da); // descending
        });
    }

    public Cluster GetClosestNeighborCluster() {
        if (!HasNeighbors())
            throw new InvalidOperationException("Cluster has no neighbors.");

        return _neighbors[^1];
    }

    /// <summary>
    /// Adds a sector to the cluster and to the growable sectors if applicable.
    /// </summary>
    /// <param name="sector"></param>
    public void AddSector(Sector sector) {
        ArgumentNullException.ThrowIfNull(sector);

        bool added = false;
        if (!_sectors.Contains(sector)) {
            sector.Claim(this, Race.Name);
            _sectors.Add(sector);
            added = true;
        }

        if (sector.CanGrow())
            AddGrowableSector(sector);

        if (added)
            SectorAdded?.Invoke(sector);
    }
}