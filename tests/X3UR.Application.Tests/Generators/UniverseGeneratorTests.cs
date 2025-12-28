using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X3UR.Application.Services;
using X3UR.Domain.DTOs;
using X3UR.Domain.Enums;
using X3UR.Infrastructure.Randoms;

namespace X3UR.Application.Tests.Generators;

public class UniverseGeneratorTests {
    private UniverseSettingsDto _settings;

    public UniverseGeneratorTests() {
        RandomProvider random = new(new SeedProvider() { Seed = 15 });
        byte witdh = (byte)random.Next(5, 25);
        byte height = (byte)random.Next(5, 21);

        _settings = new UniverseSettingsDto {
            TotalSize = (short)(witdh * height),
            Width = witdh,
            Height = height,
            RaceSettings = new List<RaceSettingDto> {
                new RaceSettingDto {
                    Name = RaceNames.Argon,
                    ColorHex = "#FF0000",
                    MaxRaceSize = 30,
                    MaxClusters = 5,
                    MaxClusterSize = 10
                }
            }
        };
    }

    [Fact]

}