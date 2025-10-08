using X3UR.Domain.DTOs;
using X3UR.Domain.Models;

namespace X3UR.Domain.Interfaces;
/// <summary>
/// Generiert ein Universum basierend auf den Einstellungen.
/// </summary>
public interface IUniverseGenerator {
    /// <summary>
    /// Erzeugt ein Universum nach den übergebenen Einstellungen.
    /// </summary>
    Task<Universe> GenerateAsync(UniverseSettingsDto settings, IProgress<double>? progress = null, CancellationToken ct = default);
}