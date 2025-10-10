using System.Diagnostics;
using X3UR.Domain.DTOs;
using X3UR.Domain.Enums;
using X3UR.Domain.Interfaces;
using X3UR.Domain.Models;

namespace X3UR.Application.Generators;
public class UniverseGenerator : IUniverseGenerator
{
    private readonly IRandomProvider _rnd;
    private Universe _universe;
    private UniverseSettingsDto _settings;
    private int _minDistSameRace;
    private int _minDistDiffRace;

    // Nur für UniverseGenerator relevant
    private readonly struct SectorCoord
    {
        public byte X { get; }
        public byte Y { get; }

        public SectorCoord(byte x, byte y)
        {
            X = x;
            Y = y;
        }

        // Optional: Equals/GetHashCode für HashSet-Nutzung
        public override bool Equals(object obj) =>
            obj is SectorCoord other && X == other.X && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    public UniverseGenerator(IRandomProvider rnd) {
        _rnd = rnd;
    }

    /// <summary>
    /// Erzeugt ein Universum anhand der übergebenen Einstellungen.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="progress"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task<Universe> GenerateAsync(UniverseSettingsDto settings, IProgress<double> progress = null, CancellationToken ct = default) {
        _universe = new Universe() {
            Map = new Sector[settings.Height, settings.Width],
            Clusters = new List<Cluster>(settings.RaceSettings.Sum(r => r.MaxClusters))
        };
        _settings = settings;

        _minDistSameRace = (int)Math.Round(settings.TotalSize * (12.5f / 374), MidpointRounding.AwayFromZero);
        _minDistDiffRace = (int)Math.Round(settings.TotalSize * (1.5f / 374), MidpointRounding.AwayFromZero);

        FillUniverseWihtSectors();
        SetClusterStartPositions();
        // 3) Cluster wachsen lassen

        return Task.FromResult(_universe);
    }

    // Füllt das Universum mit leeren Sektoren.
    private void FillUniverseWihtSectors() {
        for (byte y = 0; y < _settings.Height; y++)
        for (byte x = 0; x < _settings.Width; x++)
            _universe.Map[y, x] = new Sector() { X = x, Y = y };
    }

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

    private Dictionary<RaceNames, List<SectorCoord>> InitClusterPositionsPerRace() {
        var dict = new Dictionary<RaceNames, List<SectorCoord>>();
        foreach (var raceSetting in _settings.RaceSettings) {
            if (raceSetting.MaxClusters > 0)
                dict[raceSetting.Name] = new List<SectorCoord>(raceSetting.MaxClusters);
        }
        return dict;
    }

    private void PlaceClusterForRace(RaceNames raceName, Dictionary<RaceNames, int> raceClusterCounts, Dictionary<RaceNames, List<SectorCoord>> clusterPositions_PerSameRaces, HashSet<SectorCoord> freeSectorCoords) {
        var clusterPositions_SameRace = clusterPositions_PerSameRaces[raceName];
        var freeSectorCoordsWithCalcDistances = GetValidFreeSectorCoordsForRace(freeSectorCoords, clusterPositions_SameRace, raceName);

        if (freeSectorCoordsWithCalcDistances.Count == 0)
            return;

        var randomSector = GetRandomFreeSector(freeSectorCoordsWithCalcDistances);
        var cluster = new Cluster()
        {
            X = randomSector.X,
            Y = randomSector.Y,
            Race = _settings.RaceSettings.First(r => r.Name == raceName)
        };
        randomSector.Claim(cluster, cluster.Race.Name);
        AddAdjacentFreeSectorsToSector(randomSector);
        cluster.Sectors.Add(randomSector);
        AddClusterNeighborsInRange(cluster, 10);
        _universe.Clusters.Add(cluster);
        clusterPositions_SameRace.Add(new SectorCoord(randomSector.X, randomSector.Y));
        freeSectorCoords.Remove(new SectorCoord(randomSector.X, randomSector.Y));
        RemoveSectorsInRadiusOfDifferentRace(freeSectorCoords, new SectorCoord(randomSector.X, randomSector.Y));

        UpdateClusterCountForRace(raceName, raceClusterCounts);
    }

    private HashSet<SectorCoord> GetValidFreeSectorCoordsForRace(
        HashSet<SectorCoord> freeSectorCoords,
        List<SectorCoord> clusterPositions_SameRace,
        RaceNames raceName)
    {
        var result = new HashSet<SectorCoord>(freeSectorCoords);
        foreach (var coord in clusterPositions_SameRace)
            RemoveSectorsInRadiusOfSameRace(result, raceName, coord);
        return result;
    }

    private void UpdateClusterCountForRace(RaceNames raceName, Dictionary<RaceNames, int> raceClusterCounts)
    {
        raceClusterCounts[raceName]--;
        if (raceClusterCounts[raceName] == 0)
            raceClusterCounts.Remove(raceName);
    }

    // Wählt zufällig einen freien Sektor aus der Liste der freien Sektoren aus und entfernt diesen aus der Liste und gibt ihn zurück.
    private Sector GetRandomFreeSector(HashSet<SectorCoord> freeSectorCoordsWithCalcDistances) {
        int randomIndex = _rnd.Next(freeSectorCoordsWithCalcDistances.Count);
        var coord = freeSectorCoordsWithCalcDistances.ElementAt(randomIndex);
        freeSectorCoordsWithCalcDistances.Remove(coord);
        return _universe.Map[coord.Y, coord.X];
    }

    // Entfernt alle Sektoren im angegebenen Radius des Koordinatenpunktes, aus dem HashSet der freien Sektoren.
    // Der Radius hängt von der Anzahl der Cluster dieser Rasse ab.
    private void RemoveSectorsInRadiusOfSameRace(HashSet<SectorCoord> freeSectorCoordsWithCalcDistances, RaceNames raceName, SectorCoord coord) {
        int maxClusters = _settings.RaceSettings.First(r => r.Name == raceName).MaxClusters;
        int minDistance = (int)Math.Round(2.0 / maxClusters * _minDistSameRace, MidpointRounding.AwayFromZero);

        var toRemove = new List<SectorCoord>();
        foreach (var sector in freeSectorCoordsWithCalcDistances) {
            int dx = sector.X - coord.X;
            int dy = sector.Y - coord.Y;
            int distanceSquared = dx * dx + dy * dy;
            if (distanceSquared < minDistance)
                toRemove.Add(sector);
        }
        foreach (var sector in toRemove)
            freeSectorCoordsWithCalcDistances.Remove(sector);
    }

    // Entfernt alle Sektoren im angegebenen Radius des Koordinatenpunktes, aus dem HashSet der freien Sektoren.
    private void RemoveSectorsInRadiusOfDifferentRace(HashSet<SectorCoord> freeSectorCoords, SectorCoord coord) {
        var toRemove = new List<SectorCoord>();
        foreach (var sector in freeSectorCoords) {
            int dx = sector.X - coord.X;
            int dy = sector.Y - coord.Y;
            int distanceSquared = dx * dx + dy * dy;
            if (distanceSquared < _minDistDiffRace)
                toRemove.Add(sector);
        }
        foreach (var sector in toRemove)
            freeSectorCoords.Remove(sector);
    }

    // Fügt nur Horizontal und Vertikal angrenzende freie Sektoren zur FreeSpaces-Liste des Sektors hinzu
    // und fügt den Sektor zur Claimer-Liste der angrenzenden freien Sektoren hinzu.
    private void AddAdjacentFreeSectorsToSector(Sector sector) {
        int x = sector.X;
        int y = sector.Y;

        if (x > 0) {
            Sector leftSector = _universe.Map[y, x - 1];
            if (leftSector.Cluster == null) {
                sector.AddFreeSector(leftSector);
                leftSector.AddClaimer(sector);
            }
        }
        if (x < _settings.Width - 1) {
            Sector rightSector = _universe.Map[y, x + 1];
            if (rightSector.Cluster == null) {
                sector.AddFreeSector(rightSector);
                rightSector.AddClaimer(sector);
            }
        }
        if (y > 0) {
            Sector topSector = _universe.Map[y - 1, x];
            if (topSector.Cluster == null) {
                sector.AddFreeSector(topSector);
                topSector.AddClaimer(sector);
            }
        }
        if (y < _settings.Height - 1) {
            Sector bottomSector = _universe.Map[y + 1, x];
            if (bottomSector.Cluster == null) {
                sector.AddFreeSector(bottomSector);
                bottomSector.AddClaimer(sector);
            }
        }
    }


    // Zeigt eine 2D-Matrix mit den Startpositionen der Cluster und die Anzahl der Cluster an.
    private void PrintClusterStartPositions() {
        int clusterCount = 0;

        for (byte y = 0; y < _settings.Height; y++) {
            for (byte x = 0; x < _settings.Width; x++) {
                Sector sector = _universe.Map[y, x];
                if (sector.Cluster != null) {
                    Debug.Write("O");
                    clusterCount++;
                } else {
                    Debug.Write("X");
                }
            }
            Debug.WriteLine("");
        }
        Debug.WriteLine($"Anzahl Cluster: {clusterCount}");
    }

    // Fügt alle Cluster des Universums, die im angegebenen Bereich des Clusters liegen, als Nachbarn hinzu.
    private void AddClusterNeighborsInRange(Cluster cluster, int range) {
        if (_universe.Clusters.Count < 2) return;
        int rangeSquared = range * range;
        int dx;
        int dy;
        int distanceSquared;

        foreach (var otherCluster in _universe.Clusters) {
            if (otherCluster == cluster)
                continue;
            dx = otherCluster.X - cluster.X;
            dy = otherCluster.Y - cluster.Y;
            distanceSquared = dx * dx + dy * dy;
            if (distanceSquared <= rangeSquared) {
                cluster.AddNeighbor(otherCluster);
                otherCluster.AddNeighbor(cluster);
            }
        }
    }
}