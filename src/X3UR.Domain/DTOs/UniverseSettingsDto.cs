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
    public short CurrentSize { get; init; }
    public short CurrentClusters { get; init; }
    public short CurrentClusterSize { get; init; }
    public bool IsActive { get; init; }
}