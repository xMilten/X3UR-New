using System.Threading.Tasks;
using System.Windows.Input;
using X3UR.Domain.DTOs;
using X3UR.Domain.Interfaces;
using X3UR.Domain.Models;
using X3UR.Infrastructure.Commands;
using X3UR.UI.ViewModels.UserSettings;
using X3UR.UI.ViewModels.UserSettings.SettingsTab;

namespace X3UR.UI.ViewModels;
public class MainWindowViewModel {
    private readonly IUniverseGenerator _universeGenerator;
    public string Status { get; private set; } = "";
    public SeedViewModel SeedVm { get; }
    public UniverseSettingsTabViewModel SettingsVm { get; }

    public ICommand GenerateCommand { get; }

    public MainWindowViewModel(SeedViewModel seedVm, UniverseSettingsTabViewModel settingsVm, IUniverseGenerator universeGenerator) {
        SeedVm = seedVm;
        SettingsVm = settingsVm;
        _universeGenerator = universeGenerator;
        GenerateCommand = new RelayCommand(async () => await OnGenerateAsync());
    }

    private async Task OnGenerateAsync() {
        UniverseSettingsDto settings = SettingsVm.ToDto();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var progress = new Progress<double>(p => Status = $"Generating... {p:P0}");

        Status = "Generierung läuft...";
        Universe universe = await Task.Run(() => _universeGenerator.GenerateAsync(settings, progress, cts.Token));

        Status = "Generierung abgeschlossen!";
    }
}