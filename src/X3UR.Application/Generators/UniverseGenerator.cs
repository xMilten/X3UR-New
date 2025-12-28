using System.Diagnostics;
using X3UR.Domain.DTOs;
using X3UR.Domain.Enums;
using X3UR.Domain.Interfaces;
using X3UR.Domain.Models;
using X3UR.Domain.Utilities;

namespace X3UR.Application.Generators;
public class UniverseGenerator : IUniverseGenerator {
    private readonly IRandomProvider _rnd;

    private Universe _universe;
    private UniverseSettingsDto _settings;
    private Dictionary<RaceNames, RaceSettingDto> _raceSettingsByName;
    private Dictionary<RaceNames, int> _raceSizes;
    private int _minDistSameRace;
    private int _minDistDiffRace;

    // Nur für UniverseGenerator relevant
    private readonly record struct SectorCoord(byte X, byte Y);
    public UniverseGenerator(IRandomProvider rnd) => _rnd = rnd;

    /// <summary>
    /// Erzeugt ein Universum anhand der übergebenen Einstellungen.
    /// </summary>
    /// <param name="settings"></param>
    public Task<Universe> GenerateAsync(Universe universe, UniverseSettingsDto settings) {
        _universe = universe;
        _settings = settings;
        _raceSettingsByName = _settings.RaceSettings.ToDictionary(r => r.Name);
        _raceSizes = _settings.RaceSettings.ToDictionary(r => r.Name, _ => 0);

        _minDistSameRace = (int)Math.Round(settings.TotalSize * (12.5f / 374), MidpointRounding.AwayFromZero);
        _minDistDiffRace = (int)Math.Round(settings.TotalSize * (1.5f / 374), MidpointRounding.AwayFromZero);

        SetClusterStartPositions();
        SortAllClusterNeighborsDesc();
        GrowClusters();

        return Task.FromResult(_universe);
    }

    // Setzt die Startpositionen der Cluster im Universum.
    private void SetClusterStartPositions() {
        var raceClusterCounts = GetRaceClusterCounts();
        var clusterPositions_PerSameRaces = InitClusterPositionsPerRace();
        var freeSectorCoords = new HashSet<SectorCoord>(GetAllFreeSectorCoords());

        while (raceClusterCounts.Count > 0) {
            var racesInRound = _rnd.Shuffle(raceClusterCounts.Keys).ToList();

            foreach (var raceName in racesInRound) {
                PlaceClusterForRace(
                    raceName,
                    raceClusterCounts,
                    clusterPositions_PerSameRaces,
                    freeSectorCoords
                );
            }
        }

        PrintClusterStartPositions();
    }

    // Platziert einen Cluster für die angegebene Rasse im Universum.
    private void PlaceClusterForRace(RaceNames raceName, Dictionary<RaceNames, int> raceClusterCounts, Dictionary<RaceNames, List<SectorCoord>> clusterPositions_PerSameRaces, HashSet<SectorCoord> freeSectorCoords) {
        var clusterPositions_SameRace = clusterPositions_PerSameRaces[raceName];
        var freeSectorCoordsWithCalcDistances = GetValidFreeSectorCoordsForRace(freeSectorCoords, clusterPositions_SameRace, raceName);

        if (freeSectorCoordsWithCalcDistances.Count == 0)
            return;

        var randomSector = GetRandomFreeSector(freeSectorCoordsWithCalcDistances);
        var cluster = new Cluster() {
            X = randomSector.X,
            Y = randomSector.Y,
            Race = _raceSettingsByName[raceName]
        };
        _universe.AddCluster(cluster);
        cluster.AddSector(randomSector);
        _raceSizes[raceName]++;
        clusterPositions_SameRace.Add(new SectorCoord(randomSector.X, randomSector.Y));
        freeSectorCoords.Remove(new SectorCoord(randomSector.X, randomSector.Y));
        RemoveSectorsInRadiusOfDifferentRace(freeSectorCoords, new SectorCoord(randomSector.X, randomSector.Y));

        UpdateClusterCountForRace(raceName, raceClusterCounts);
    }

    private void SortAllClusterNeighborsDesc() {
        foreach (Cluster cluster in _universe.Clusters)
            cluster.SortNeighborsDesc();
    }

    // Lässt alle Cluster im Universum wachsen (Orchestrierung über alle Rassen).
    private void GrowClusters() {
        var raceClusters = GetRaceClusters();

        while (raceClusters.Count > 0) {
            var racesInRound = _rnd.Shuffle(raceClusters.Keys).ToList();

            foreach (var raceName in racesInRound) {
                if (!raceClusters.TryGetValue(raceName, out var clusters) || clusters.Count == 0) {
                    raceClusters.Remove(raceName);
                    continue;
                }

                var raceSetting = _raceSettingsByName[raceName];
                if (_raceSizes[raceName] >= raceSetting.MaxRaceSize) {
                    raceClusters.Remove(raceName);
                    continue;
                }

                GrowClustersForRace(raceName, clusters, raceClusters);

                if (!raceClusters.TryGetValue(raceName, out var remaining) || remaining.Count == 0
                    || _raceSizes[raceName] >= raceSetting.MaxRaceSize) {
                    raceClusters.Remove(raceName);
                }
            }
        }

        PrintClusterStartPositions();
    }

    // Führt einen Wachstums-Schritt für eine Rasse aus: wählt einen zufälligen Cluster und expandiert ihn.
    private void GrowClustersForRace(RaceNames raceName, List<Cluster> clusters, Dictionary<RaceNames, List<Cluster>> raceClusters) {
        var raceSetting = _raceSettingsByName[raceName];

        if (_raceSizes[raceName] >= raceSetting.MaxRaceSize) {
            raceClusters.Remove(raceName);
            return;
        }

        int idx = _rnd.Next(clusters.Count);
        Cluster cluster = clusters[idx];

        if (!cluster.CanGrow()) {
            clusters.RemoveAt(idx);
            if (clusters.Count == 0)
                raceClusters.Remove(raceName);
            return;
        }

        Sector growableSector;
        Sector freeSector;

        if (cluster.HasNeighbors()) {
            growableSector = cluster.GetGrowableSectorTowardsNeighbor();
            freeSector = growableSector?.GetFreeSectorTowardsNeighbor();
        } else {
            growableSector = cluster.GetRandomGrowableSector(_rnd);
            freeSector = growableSector?.GetRandomFreeSector(_rnd);
        }

        if (growableSector == null || freeSector == null) {
            if (idx >= 0 && idx < clusters.Count && clusters[idx] == cluster)
                clusters.RemoveAt(idx);
            else
                clusters.RemoveAll(c => !c.CanGrow());
            if (clusters.Count == 0)
                raceClusters.Remove(raceName);
            return;
        }

        _universe.ClaimAndExpand(freeSector, cluster);
        _raceSizes[raceName]++;

        clusters.RemoveAll(c => !c.CanGrow());

        if (clusters.Count == 0 || _raceSizes[raceName] >= raceSetting.MaxRaceSize)
            raceClusters.Remove(raceName);
    }

    // Zeigt eine 2D-Matrix mit den Startpositionen der Cluster und die Anzahl der Cluster an.
    private void PrintClusterStartPositions() {
        int clusterCount = 0;
        string clusterNeighbors = "";

        for (byte y = 0; y < _universe.Height; y++) {
            for (byte x = 0; x < _universe.Width; x++) {
                Sector sector = _universe.GetSector(x, y);
                if (sector.Cluster != null) {
                    Debug.Write(Enum.GetName(typeof(RaceCharakters), sector.Race));
                    clusterNeighbors += $"Cluster at ({sector.X},{sector.Y}) has {(sector.Cluster.HasNeighbors() ? $"{sector.Cluster.GetNeighborCount()} neighbors" : "no neighbors")}\n";
                    clusterCount++;
                } else {
                    Debug.Write(" ");
                }
            }
            Debug.WriteLine("");
        }
        Debug.WriteLine($"Anzahl Cluster: {clusterCount}");
        Debug.WriteLine($"Anzahl Cluster laut Universum: {_universe.Clusters.Count}");
        Debug.WriteLine(clusterNeighbors);
    }

    // ----- Hilfsmethoden -----------------------------------------------------------------------------------

    // Gibt ein Dictionary mit den Rassen und der Anzahl der Cluster zurück, die jede Rasse haben soll.
    private Dictionary<RaceNames, int> GetRaceClusterCounts() {
        Dictionary<RaceNames, int> raceClusterCounts = new();

        foreach (RaceSettingDto raceSetting in _settings.RaceSettings) {
            if (raceSetting.MaxClusters > 0)
                raceClusterCounts[raceSetting.Name] = raceSetting.MaxClusters;
        }

        return raceClusterCounts;
    }

    // Gibt eine Liste aller freien Sektoren im Universum zurück.
    private List<SectorCoord> GetAllFreeSectorCoords() {
        var sectorCoords = new List<SectorCoord>(_settings.TotalSize);
        for (byte y = 0; y < _settings.Height; y++)
            for (byte x = 0; x < _settings.Width; x++)
                sectorCoords.Add(new SectorCoord(x, y));
        return sectorCoords;
    }

    private Dictionary<RaceNames, List<SectorCoord>> InitClusterPositionsPerRace() {
        var dict = new Dictionary<RaceNames, List<SectorCoord>>();
        foreach (var raceSetting in _settings.RaceSettings) {
            if (raceSetting.MaxClusters > 0)
                dict[raceSetting.Name] = new List<SectorCoord>(raceSetting.MaxClusters);
        }
        return dict;
    }

    private HashSet<SectorCoord> GetValidFreeSectorCoordsForRace(HashSet<SectorCoord> freeSectorCoords, List<SectorCoord> clusterPositions_SameRace, RaceNames raceName) {
        var result = new HashSet<SectorCoord>(freeSectorCoords);
        foreach (var coord in clusterPositions_SameRace)
            RemoveSectorsInRadiusOfSameRace(result, raceName, coord);
        return result;
    }

    private void UpdateClusterCountForRace(RaceNames raceName, Dictionary<RaceNames, int> raceClusterCounts) {
        raceClusterCounts[raceName]--;
        if (raceClusterCounts[raceName] == 0)
            raceClusterCounts.Remove(raceName);
    }

    // Wählt zufällig einen freien Sektor aus der Liste der freien Sektoren aus und entfernt diesen aus der Liste und gibt ihn zurück.
    private Sector GetRandomFreeSector(HashSet<SectorCoord> freeSectorCoordsWithCalcDistances) {
        // Convert to list once for O(1) random access
        var list = freeSectorCoordsWithCalcDistances.ToList();
        int randomIndex = _rnd.Next(list.Count);
        var coord = list[randomIndex];
        freeSectorCoordsWithCalcDistances.Remove(coord);
        return _universe.GetSector(coord.X, coord.Y);
    }

    // Entfernt alle Sektoren im angegebenen Radius des Koordinatenpunktes, aus dem HashSet der freien Sektoren.
    // Der Radius hängt von der Anzahl der Cluster dieser Rasse ab.
    private void RemoveSectorsInRadiusOfSameRace(HashSet<SectorCoord> freeSectorCoordsWithCalcDistances, RaceNames raceName, SectorCoord sectorCoord) {
        int maxClusters = _raceSettingsByName[raceName].MaxClusters;
        int minDistance = (int)Math.Round(2.0 / maxClusters * _minDistSameRace, MidpointRounding.AwayFromZero);
        var toRemove = new List<SectorCoord>();

        foreach (var freeSectorCoord in freeSectorCoordsWithCalcDistances) {
            int distanceSquared = DistanceHelper.DistanceSquared(freeSectorCoord.X, freeSectorCoord.Y, sectorCoord.X, sectorCoord.Y);

            if (distanceSquared < minDistance)
                toRemove.Add(freeSectorCoord);
        }
        foreach (var sector in toRemove)
            freeSectorCoordsWithCalcDistances.Remove(sector);
    }

    // Entfernt alle Sektoren im angegebenen Radius des Koordinatenpunktes, aus dem HashSet der freien Sektoren.
    private void RemoveSectorsInRadiusOfDifferentRace(HashSet<SectorCoord> freeSectorCoords, SectorCoord sectorCoord) {
        var toRemove = new List<SectorCoord>();

        foreach (var freeSectorCoord in freeSectorCoords) {
            int distanceSquared = DistanceHelper.DistanceSquared(freeSectorCoord.X, freeSectorCoord.Y, sectorCoord.X, sectorCoord.Y);

            if (distanceSquared < _minDistDiffRace)
                toRemove.Add(freeSectorCoord);
        }
        foreach (var sector in toRemove)
            freeSectorCoords.Remove(sector);
    }

    /// <summary>
    /// Get all race clusters that can still grow.
    /// Sorted by race name.
    /// </summary>
    private Dictionary<RaceNames, List<Cluster>> GetRaceClusters() {
        Dictionary<RaceNames, List<Cluster>> raceClusters = new();

        foreach (Cluster cluster in _universe.Clusters) {
            RaceNames raceName = cluster.Race.Name;

            // Skip clusters that cannot grow anymore due to race size limit reached
            if (_raceSizes[raceName] >= cluster.Race.MaxRaceSize) continue;

            // Skip clusters that cannot grow anymore due to cluster size limit reached
            if (!cluster.CanGrow()) continue;

            if (!raceClusters.TryGetValue(cluster.Race.Name, out var list)) {
                list = new List<Cluster>();
                raceClusters[cluster.Race.Name] = list;
            }

            list.Add(cluster);
        }

        return raceClusters;
    }

    public enum RaceCharakters : byte {
        None,
        A = 1,
        B = 2,
        S = 3,
        P = 4,
        T = 5,
        X = 6,
        K = 7,
        p = 8,
        U = 14,
        t = 17,
        Y = 19
    }
}