using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X3UR.Domain.DTOs;
using X3UR.Domain.Enums;
using X3UR.Domain.Interfaces;
using X3UR.Domain.Utilities;

namespace X3UR.Domain.Models;
public class Sector {
    private List<Sector> _freeSectors;
    private List<Sector> _claimers;

    public byte X { get; }
    public byte Y { get; }
    public Cluster Cluster { get; private set; }
    public RaceNames Race { get; private set; }

    public bool CanGrow() => _freeSectors != null && _freeSectors.Count > 0;
    public bool CanBeClaimed() => _claimers != null && _claimers.Count > 0;

    public Sector(byte x, byte y) {
        X = x;
        Y = y;
    }

    public void Claim(Cluster cluster, RaceNames race) {
        ArgumentNullException.ThrowIfNull(cluster);
        Cluster = cluster;
        Race = race;
    }

    public void AddClaimer(Sector sector) {
        ArgumentNullException.ThrowIfNull(sector);
        _claimers ??= ListPool<Sector>.Rent();

        if (!_claimers.Contains(sector))
            _claimers.Add(sector);
    }

    public void AddFreeSector(Sector sector) {
        ArgumentNullException.ThrowIfNull(sector);
        _freeSectors ??= ListPool<Sector>.Rent();

        if (!_freeSectors.Contains(sector))
            _freeSectors.Add(sector);
    }

    public void RemoveFreeSpace(Sector sector) {
        if (_freeSectors == null)
            return;

        _freeSectors?.Remove(sector);

        if (_freeSectors.Count == 0) {
            ListPool<Sector>.Return(_freeSectors);
            _freeSectors = null;
            Cluster?.RemoveGrowableSector(this);
        }
    }

    public Sector GetRandomFreeSector(IRandomProvider rand) {
        if (!CanGrow())
            throw new InvalidOperationException("Sector cannot grow.");

        int index = rand.Next(0, _freeSectors.Count);
        return _freeSectors[index];
    }

    /// <summary>
    /// Get the free sector that is closest to the nearest neighbor cluster.
    /// </summary>
    public Sector GetFreeSectorTowardsNeighbor() {
        if (Cluster == null || !CanGrow() || !Cluster.HasNeighbors())
            throw new InvalidOperationException("Sector's cluster is null, sector cannot grow or cluster has no neighbors.");

        Cluster closestNeighborCluster = Cluster.GetClosestNeighborCluster();
        Sector closestFreeSector = null;
        int bestDist = int.MaxValue;

        foreach (var freeSector in _freeSectors) {
            int dx = closestNeighborCluster.X - freeSector.X;
            int dy = closestNeighborCluster.Y - freeSector.Y;
            int dist = dx * dx + dy * dy;
            if (dist < bestDist) {
                bestDist = dist;
                closestFreeSector = freeSector;
            }
        }

        return closestFreeSector;
    }

    /// <summary>
    /// Remove this sector from all its claimers' free spaces and remove all claimers from this sector.
    /// </summary>
    public void RemoveAllClaimersFromMeAndMeFromThem() {
        if (_claimers == null) return;

        foreach (var claimer in _claimers) {
            claimer.RemoveFreeSpace(this);

            if (!claimer.CanGrow()) {
                claimer.Cluster?.RemoveGrowableSector(claimer);
            }
        }

        ListPool<Sector>.Return(_claimers);
        _claimers = null;
    }
}