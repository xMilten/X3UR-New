using X3UR.Domain.Enums;

namespace X3UR.Domain.DTOs;
public class UniverseSettingsDto {
    public short TotalSize { get; init; }
    public byte Width { get; init; }
    public byte Height { get; init; }
    public IReadOnlyList<RaceSettingDto> RaceSettings { get; init; }
}

public class RaceSettingDto {
    public RaceNames Name { get; init; }
    public string ColorHex { get; init; }
    public short CurrentRaceSize { get; set; } = 0;
    public short MaxRaceSize { get; init; }
    public short CurrentClusters { get; set; } = 0;
    public short MaxClusters { get; init; }
    public short CurrentClusterSize { get; set; } = 0;
    public short MaxClusterSize { get; init; }
}