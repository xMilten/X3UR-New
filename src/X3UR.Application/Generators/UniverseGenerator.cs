using X3UR.Domain.DTOs;
using X3UR.Domain.Interfaces;
using X3UR.Domain.Models;

namespace X3UR.Application.Generators;
public class UniverseGenerator : IUniverseGenerator {
    public readonly IRandomProvider _rnd;

    public UniverseGenerator(IRandomProvider rnd) => _rnd = rnd;

    public Universe Generate(UniverseSettingsDto dto) {
        // Hier kommt die Generierung des Universums
        return new Universe();
    }
}