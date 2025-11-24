using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using X3UR.Domain.DTOs;
using X3UR.Domain.Utilities;

namespace X3UR.Domain.Models;
public class Universe {
    private Sector[,] _map;
    public List<Cluster> Clusters { get; private set; }
    public short Width => (short)_map.GetLength(1);
    public short Height => (short)_map.GetLength(0);

    /// <summary>
    /// Erstellt ein neues Universum anhand der übergebenen Einstellungen.
    /// </summary>
    /// <param name="settings"></param>
    public Universe(UniverseSettingsDto settings) {
        _map = new Sector[settings.Height, settings.Width];
        Clusters = new List<Cluster>(settings.RaceSettings.Sum(r => r.MaxClusters));

        for (byte y = 0; y < settings.Height; y++)
        for (byte x = 0; x < settings.Width; x++)
            _map[y, x] = new Sector(x, y);
    }

    /// <summary>
    /// Versucht den Sektor an den übergebenen Koordinaten zu bekommen.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sector"></param>
    public Sector GetSector(int x, int y) {
        if (x < 0 || x >= _map.GetLength(1))
            throw new ArgumentOutOfRangeException(nameof(x), $"x={x} liegt außerhalb der Universumsgrenzen.");
        if (y < 0 || y >= _map.GetLength(0))
            throw new ArgumentOutOfRangeException(nameof(y), $"y={y} liegt außerhalb der Universumsgrenzen.");

        return _map[y, x];
    }

    public void AddCluster(Cluster cluster, int neighborRange = 10) {
        ArgumentNullException.ThrowIfNull(cluster);
        Clusters.Add(cluster);
        RegisterClusterNeighborsInRange(cluster, neighborRange);
        cluster.SectorAdded += OnClusterSectorAdded;
    }

    private void OnClusterSectorAdded(Sector sector) {
        if (sector == null) return;
        RegisterAdjacentFreeSectors(sector);
    }

    /// <summary>
    /// Gibt alle freien angrenzenden Sektoren des übergebenen Sektors zurück.
    /// </summary>
    /// <param name="sector"></param>
    public IEnumerable<Sector> GetFreeAdjacentSectors(Sector sector) {
        if (sector == null)
            yield break;

        // Left
        if (sector.X > 0) {
            var s = _map[sector.Y, sector.X - 1];
            if (s.Cluster == null)
                yield return s;
        }
        // Right
        if (sector.X < Width - 1) {
            var s = _map[sector.Y, sector.X + 1];
            if (s.Cluster == null)
                yield return s;
        }
        // Up
        if (sector.Y > 0) {
            var s = _map[sector.Y - 1, sector.X];
            if (s.Cluster == null)
                yield return s;
        }
        // Down
        if (sector.Y < Height - 1) {
            var s = _map[sector.Y + 1, sector.X];
            if (s.Cluster == null)
                yield return s;
        }
    }

    /// <summary>
    /// Registriert die direkten (4‑Weg) freien Nachbarbeziehungen:
    /// owner erhält die freien Sektoren in _freeSpaces und die jeweiligen freien Sektoren erhalten owner als Claimer.
    /// </summary>
    /// <param name="owner"></param>
    private void RegisterAdjacentFreeSectors(Sector owner) {
        if (owner == null) return;

        foreach (Sector adjacent in GetFreeAdjacentSectors(owner)) {
            owner.AddFreeSector(adjacent);
            adjacent.AddClaimer(owner);
        }
    }

    /// <summary>
    /// Atomare Operation: Claim auf freeSector durch cluster und alle Folge‑Updates in Universe/Cluster/Sector.
    /// - setzt Claim,
    /// - fügt den Sektor dem Cluster hinzu,
    /// - entfernt Claimer‑Beziehungen,
    /// - registriert angrenzende freie Sektoren.
    /// </summary>
    /// <param name="freeSector"></param>
    /// <param name="cluster"></param>
    public void ClaimAndExpand(Sector freeSector, Cluster cluster) {
        ArgumentNullException.ThrowIfNull(freeSector);
        ArgumentNullException.ThrowIfNull(cluster);

        cluster.AddSector(freeSector);
        freeSector.RemoveAllClaimersFromMeAndMeFromThem();

        if (freeSector.CanGrow())
            cluster.AddGrowableSector(freeSector);
    }

    /// <summary>
    /// Gibt alle Cluster im angegebenen Bereich (Quadrat der Distanz) um den übergebenen Cluster zurück.
    /// </summary>
    /// <param name="cluster"></param>
    /// <param name="rangeSquared"></param>
    public IEnumerable<Cluster> GetClustersInRange(Cluster cluster, int range) {
        if (cluster == null || Clusters == null)
            yield break;

        int rangeSquared = range * range;

        foreach (Cluster other in Clusters) {
            if (other == cluster)
                continue;

            int dist = DistanceHelper.DistanceSquared(other.X, other.Y, cluster.X, cluster.Y);

            if (dist <= rangeSquared)
                yield return other;
        }
    }

    private void RegisterClusterNeighborsInRange(Cluster cluster, int neighborRange) {
        ArgumentNullException.ThrowIfNull(cluster);
        if (Clusters.Count < 2) return;

        foreach (Cluster neighbor in GetClustersInRange(cluster, neighborRange)) {
            cluster.AddNeighbor(neighbor);
            neighbor.AddNeighbor(cluster);
        }
    }
}