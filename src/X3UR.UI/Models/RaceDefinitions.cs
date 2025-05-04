using System.Windows.Media;
using X3UR.Domain.Enums;

namespace X3UR.UI.Models;
public static class RaceDefinitions {
    public static IReadOnlyList<RaceDefinition> All { get; } = new[] {
        new RaceDefinition(RaceNames.Argon, HexToColor("#1e1ee1"), 32, 3, 16, true),
        new RaceDefinition(RaceNames.Boron, HexToColor("#1ee11e"), 27, 5, 7, true),
        new RaceDefinition(RaceNames.Split, HexToColor("#e11ee1"), 31, 4, 13, true),
        new RaceDefinition(RaceNames.Paranid, HexToColor("#1ee1e1"), 27, 2, 14, true),
        new RaceDefinition(RaceNames.Teladi, HexToColor("#e1e11e"), 30, 4, 9, true),
        new RaceDefinition(RaceNames.Xenon, HexToColor("#e11e1e"), 10, 6, 4, true),
        new RaceDefinition(RaceNames.Khaak, HexToColor("#701ee1"), 3, 3, 1, true),
        new RaceDefinition(RaceNames.Piraten, HexToColor("#e1701e"), 20, 8, 8, true),
        new RaceDefinition(RaceNames.Unbekannt, HexToColor("#a0a0a0"), 13, 11, 2, true),
        new RaceDefinition(RaceNames.Terraner, HexToColor("#dfffbf"), 21, 1, 21, true),
        new RaceDefinition(RaceNames.Yaki, HexToColor("#ffbfdf"), 3, 1, 3, true)
    };

    private static Color HexToColor(string hex) {
        return (Color)ColorConverter.ConvertFromString(hex);
    }
}

public record RaceDefinition(
    RaceNames Name,
    Color Color,
    int DefaultSize,
    int DefaultClusters,
    int DefaultClusterSize,
    bool IsDefaultActive
);