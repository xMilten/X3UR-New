using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X3UR.Domain.DTOs;
using X3UR.Domain.Enums;
using X3UR.Domain.Interfaces;

namespace X3UR.Domain.Models;
public class Sector {
    private List<Sector> _freeSpaces;
    private List<Sector> _claimers;

    public byte X { get; init; }
    public byte Y { get; init; }
    public Cluster Cluster { get; private set; }
    public RaceNames Race { get; private set; }

    public void Claim(Cluster cluster, RaceNames race) {
        Cluster = cluster;
        Race = race;
    }

    public void AddFreeSector(Sector sector) {
        _freeSpaces ??= new List<Sector>();
        _freeSpaces.Add(sector);
    }

    public Sector GetRandomFreeSector(IRandomProvider rand) {
        if (!CanGrow()) return null;
        int index = rand.Next(0, _freeSpaces.Count);
        return _freeSpaces[index];
    }

    // Gib den den freien Sektor, der am nächsten zum Nachbarcluster meines Clusters liegt
    public Sector GetBestFreeSectorTowardsNeighbor() {
        if (Cluster == null) return null;

        Cluster closestNeighborCluster = Cluster.GetClosestNeighborCluster();

        if (!CanGrow() || closestNeighborCluster == null) return null;
        Sector bestSector = null;
        int bestDist = int.MaxValue;

        foreach (var sector in _freeSpaces) {
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

    public void RemoveFreeSpace(Sector sector) {
        _freeSpaces?.Remove(sector);
    }

    public void AddClaimer(Sector sector) {
        _claimers ??= new List<Sector>();
        _claimers.Add(sector);
    }

    /// <summary>
    /// Entfernt alle Sektoren, die diesen Sektor als freien Sektor beanspruchen und entfernt diesen Sektor als freien Sektor von allen Sektoren, die diesen Sektor beanspruchen.
    /// Wenn ein Sektor danach nicht mehr wachsen kann, wird er aus seinem Cluster als wachsender Sektor entfernt.
    /// </summary>
    public void RemoveAllClaimersFromMeAndMeFromThem() {
        if (_claimers == null) return;

        foreach (var claimer in _claimers) {
            claimer.RemoveFreeSpace(this);

            if (!claimer.CanGrow()) {
                claimer.Cluster?.RemoveGrowableSector(claimer);
            }
        }
        _claimers.Clear();
        _claimers = null;
    }

    public bool CanGrow() => _freeSpaces != null && _freeSpaces.Count > 0;
    public bool CanBeClaimed() => _claimers != null && _claimers.Count > 0;
}