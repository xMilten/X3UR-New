using System.Diagnostics;
using X3UR.Domain.DTOs;
using X3UR.Domain.Enums;
using X3UR.Domain.Interfaces;
using X3UR.Domain.Models;

namespace X3UR.Application.Generators;
public class UniverseGenerator : IUniverseGenerator {
    private readonly IRandomProvider _rnd;
    private Universe _universe;
    private UniverseSettingsDto _settings;
    private int _minDistSameRace;
    private int _minDistDiffRace;

    public UniverseGenerator(IRandomProvider rnd) {
        _rnd = rnd;
    }

    public Task<Universe> GenerateAsync(UniverseSettingsDto settings, IProgress<double> progress = null, CancellationToken ct = default) {
        _universe = new Universe() { Map = new Sector[settings.Height, settings.Width] };
        _settings = settings;

        _minDistSameRace = (int)Math.Round(settings.TotalSize * (12.5f / 374), MidpointRounding.AwayFromZero);
        _minDistDiffRace = (int)Math.Round(settings.TotalSize * (1.5f / 374), MidpointRounding.AwayFromZero);

        FillUniverseWihtSectors();
        SetClusterStartPositions();
        // 3) Cluster wachsen lassen

        return Task.FromResult(_universe);
    }

    private void FillUniverseWihtSectors() {
        for (byte y = 0; y < _settings.Height; y++)
        for (byte x = 0; x < _settings.Width; x++)
            _universe.Map[y, x] = new Sector() { X = x, Y = y };
    }

    private Dictionary<RaceNames, int> GetRaceClusterCounts() {
        Dictionary<RaceNames, int> raceClusterCounts = new();

        foreach (RaceSettingDto raceSetting in _settings.RaceSettings) {
            if (raceSetting.MaxClusters > 0)
                raceClusterCounts[raceSetting.Name] = raceSetting.MaxClusters;
        }
        
        return raceClusterCounts;
    }

    private List<(byte x, byte y)> GetAllFreeSectorCoords() {
        List<(byte x, byte y)> sectorCoords = new(_settings.TotalSize);
        for (byte y = 0; y < _settings.Height; y++)
            for (byte x = 0; x < _settings.Width; x++) {
                sectorCoords.Add((x, y));
            }

        return sectorCoords;
    }

    private void SetClusterStartPositions() {
        Dictionary<RaceNames, int> raceClusterCounts = GetRaceClusterCounts();
        List<RaceNames> racesInRound;
        List<(byte x, byte y)> freeSectorCoords = GetAllFreeSectorCoords();
        List<(byte x, byte y)> freeSectorCoordsTemp;
        Dictionary<RaceNames, List<(byte x, byte y)>> clusterPositions_PerSameRaces = new();
        List<(byte x, byte y)> clusterPositions_SameRace;
        Sector randomSector;
        Cluster cluster;

        foreach (RaceSettingDto raceSetting in _settings.RaceSettings) {
            if (raceSetting.MaxClusters > 0)
                clusterPositions_PerSameRaces.Add(raceSetting.Name, new List<(byte x, byte y)>());
        }

        while (raceClusterCounts.Count > 0) {
            racesInRound = raceClusterCounts.Keys.ToList();
            racesInRound = _rnd.Shuffle(racesInRound).ToList();

            foreach (var raceName in racesInRound) {
                clusterPositions_SameRace = clusterPositions_PerSameRaces[raceName];
                freeSectorCoordsTemp = new(freeSectorCoords);

                foreach ((byte x, byte y) coord in clusterPositions_SameRace) {
                    RemoveSectorsInRadiusOfSameRace(freeSectorCoordsTemp, raceName, coord);
                }

                randomSector = GetRandomFreeSector(freeSectorCoordsTemp);
                cluster = new Cluster() {
                    X = randomSector.X,
                    Y = randomSector.Y,
                    Race = _settings.RaceSettings.First(r => r.Name == raceName)
                };
                randomSector.Claim(cluster, cluster.Race.Name);
                AddAdjacentFreeSectorsToSector(randomSector);
                cluster.Sectors.Add(randomSector);
                _universe.Clusters.Add(cluster);
                clusterPositions_SameRace.Add((randomSector.X, randomSector.Y));
                RemoveSectorsInRadiusOfDifferentRace(freeSectorCoords, (randomSector.X, randomSector.Y));

                raceClusterCounts[raceName]--;
                if (raceClusterCounts[raceName] == 0) {
                    raceClusterCounts.Remove(raceName);
                }
            }
        }

        PrintClusterStartPositions();
    }

    private Sector GetRandomFreeSector(List<(byte x, byte y)> freeSectorCoords) {
        int randomIndex = _rnd.Next(freeSectorCoords.Count);
        (byte x, byte y) coord = freeSectorCoords[randomIndex];
        freeSectorCoords[randomIndex] = freeSectorCoords[^1];
        freeSectorCoords.RemoveAt(freeSectorCoords.Count - 1);
        return _universe.Map[coord.y, coord.x];
    }

    private void RemoveSectorsInRadiusOfSameRace(List<(byte x, byte y)> freeSectorCoordsTemp, RaceNames raceName, (byte x, byte y) coord) {
        int dx;
        int dy;
        int distanceSquared;
        int maxClusters = _settings.RaceSettings.First(r => r.Name == raceName).MaxClusters;
        int minDistance = (int)Math.Round(2.0 / maxClusters * _minDistSameRace, MidpointRounding.AwayFromZero);

        for (int i = 0; i < freeSectorCoordsTemp.Count; i++) {
            dx = freeSectorCoordsTemp[i].x - coord.x;
            dy = freeSectorCoordsTemp[i].y - coord.y;
            distanceSquared = dx * dx + dy * dy;

            if (distanceSquared < minDistance) {
                freeSectorCoordsTemp.RemoveAt(i);
                i--;
            }
        }
    }

    private void RemoveSectorsInRadiusOfDifferentRace(List<(byte x, byte y)> freeSectorCoords, (byte x, byte y) coord) {
        int dx;
        int dy;
        int distanceSquared;

        for (int i = 0; i < freeSectorCoords.Count; i++) {
            dx = freeSectorCoords[i].x - coord.x;
            dy = freeSectorCoords[i].y - coord.y;
            distanceSquared = dx * dx + dy * dy;

            if (distanceSquared < _minDistDiffRace) {
                freeSectorCoords.RemoveAt(i);
                i--;
            }
        }
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


    // Zeigt eine 2D-Matrix mit den Startpositionen der Cluster im Debug an
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
}