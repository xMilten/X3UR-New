using X3UR.Domain.Enums;

namespace X3UR.Domain.DTOs;
public class UniverseSettingsDto {
    public int Width { get; init; }
    public int Height { get; init; }

    /// <summary>
    /// Für jede Rasse: Name, Color, CurrentSize, CurrentClusters, CurrentClusterSize, IsActive
    /// </summary>
    public IReadOnlyList<RaceSettingDto> RaceSettings { get; init; }
}

public class RaceSettingDto {
    public RaceNames Name { get; init; }
    public string ColorHex { get; init; }
    public short CurrentSize { get; init; }
    public short CurrentClusters { get; init; }
    public short CurrentClusterSize { get; init; }
    public bool IsActive { get; init; }
}