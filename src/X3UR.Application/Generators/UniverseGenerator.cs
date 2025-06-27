using X3UR.Domain.DTOs;
using X3UR.Domain.Interfaces;
using X3UR.Domain.Models;

namespace X3UR.Application.Generators;
public class UniverseGenerator : IUniverseGenerator {
    private readonly IRandomProvider _rnd;
    private readonly Universe _universe;
    private UniverseSettingsDto _settings;
    private float _minDistSameRace;
    private float _minDistDiffRace;

    public UniverseGenerator(IRandomProvider rnd) {
        _rnd = rnd;
    }

    public Universe Generate(UniverseSettingsDto settings) {
        var universe = new Universe() { Width = settings.Width, Height = settings.Height };
        _settings = settings;

        _minDistSameRace = settings.TotalSize * (12.5f / 374);
        _minDistDiffRace = settings.TotalSize * (1.5f / 374);

        PlaceAllStartClusters();
        GrowAllClusters();

        return _universe;
    }

    private void PlaceAllStartClusters() {
        // baue Liste von (RaceIndex, ClustersLeft)
        var clustersLeft = _settings.RaceSettings
            .Select((dto, i) => new RaceClusterState(i, dto.CurrentClusters))
            .ToList();

        // kopie aller freien Sektoren (SectorBases)
        var freeAll = CreateAllSectorBases();

        // Liste pro Rasse, um schon gesetzte Positionen derselben Rasse zu tracken
        var placedByRace = clustersLeft.ToDictionary(rc => rc.RaceIndex, rc => new List<Sector>());

        // solange noch irgendeine Rasse Clusters setzen kann
        while (clustersLeft.Any(rc => rc.ClustersLeft > 0)) {
            // für diese Runde nur die Rassen mit >0 Clusters left
            var round = clustersLeft.Where(rc => rc.ClustersLeft > 0).ToList();

            // zufällige Reihenfolge
            foreach (var rc in _rnd.Shuffle(round)) {
                // entferne einen Cluster von dieser Rasse
                rc.ClustersLeft--;

                // erzeuge frische Kopie für diff‑Abstand
                var freeForDiff = new List<Sector>(freeAll);
                // entferne Punkte, die gegen diff‑Dist verstoßen
                foreach (var sb in _universe.Sectors)
                    RemoveNearby(freeForDiff, (sb.X, sb.Y), _minDistDiffRace);

                // entferne Punkte, die gegen same‑Dist dieser Rasse verstoßen
                foreach (var sb in placedByRace[rc.RaceIndex])
                    RemoveNearby(freeForDiff, (sb.X, sb.Y), _minDistSameRace);

                // wähle zufälligen freien Platz
                var pick = GetRandomSectorBase(freeForDiff);

                // Erstelle Cluster & Sektor
                var cluster = new Cluster(rc.RaceIndex, (pick.X, pick.Y));
                _universe.Clusters.Add(cluster);
                _universe.Sectors.Add(pick);
                placedByRace[rc.RaceIndex].Add(pick);

                // aus globalen freeAll entferne alle Punkte, die diff‑Dist verletzen
                RemoveNearby(freeAll, (pick.X, pick.Y), _minDistDiffRace);
            }
        }
    }

    private void GrowAllClusters() {
        // analog: Liste von RaceClusterState mit GrowthLeft, Shuffle, FindAdjacentFree, etc.
        // ... (dein zweiter Schritt)
    }

    #region Hilfsmethoden

    private List<Sector> CreateAllSectorBases() {
        var list = new List<Sector>();
        for (byte y = 0; y < _settings.Height; y++)
        for (byte x = 0; x < _settings.Width; x++)
            list.Add(new Sector(x, y));
        return list;
    }

    private static void RemoveNearby(List<Sector> list, (byte X, byte Y) center, float minDistance) {
        list.RemoveAll(sb => DistanceOfTwoPoints2D(sb.X, sb.Y, center.X, center.Y) < minDistance);
    }

    private Sector GetRandomSectorBase(List<Sector> free) {
        int idx = _rnd.Next(free.Count);
        var sb = free[idx];
        free.RemoveAt(idx);
        return sb;
    }

    public static float DistanceOfTwoPoints2D(byte pos1X, byte pos1Y, byte pos2X, byte pos2Y) {
        return (float)Math.Round(Math.Sqrt((pos1X - pos2X) * (pos1X - pos2X) + (pos1Y - pos2Y) * (pos1Y - pos2Y)), 4);
    }

    #endregion
}

internal class RaceClusterState {
    public int RaceIndex { get; }
    public int ClustersLeft { get; set; }

    public RaceClusterState(int raceIndex, int initialClusters) {
        RaceIndex = raceIndex;
        ClustersLeft = initialClusters;
    }
}