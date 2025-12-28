using System.Threading.Tasks;
using System.Windows.Input;
using X3UR.Domain.DTOs;
using X3UR.Domain.Interfaces;
using X3UR.Domain.Models;
using X3UR.Infrastructure.Commands;
using X3UR.UI.ViewModels.UserSettings;
using X3UR.UI.ViewModels.UserSettings.SettingsTab;
using X3UR.UI.Views.UserControls.VisualUniverse;

namespace X3UR.UI.ViewModels;
public class MainWindowViewModel {
    private readonly IUniverseGenerator _universeGenerator;
    public SeedViewModel SeedVm { get; }
    public UniverseSettingsTabViewModel SettingsVm { get; }

    public ICommand GenerateCommand { get; }

    public event Action<Universe>? UniverseCreated;

    public MainWindowViewModel(SeedViewModel seedVm, UniverseSettingsTabViewModel settingsVm, IUniverseGenerator universeGenerator) {
        SeedVm = seedVm;
        SettingsVm = settingsVm;
        _universeGenerator = universeGenerator;
        GenerateCommand = new RelayCommand(async () => await OnGenerateAsync());
    }

    private async Task OnGenerateAsync() {
        UniverseSettingsDto settings = SettingsVm.ToDto();
        Universe universe = new(settings);
        UniverseCreated?.Invoke(universe);
        await Task.Run(() => _universeGenerator.GenerateAsync(universe, settings));
    }
}