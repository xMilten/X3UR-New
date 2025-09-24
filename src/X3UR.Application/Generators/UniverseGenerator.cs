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

        // 1) Universum mit leeren Sektoren füllen?
        // 2) Startpositionen der Cluster per Zufall bestimmen
        // 3) Cluster wachsen lassen

        return _universe;
    }
}