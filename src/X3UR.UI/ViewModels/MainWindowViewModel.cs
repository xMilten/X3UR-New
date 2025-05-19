using System.Windows.Input;
using X3UR.Infrastructure.Commands;
using X3UR.UI.ViewModels.UserSettings;
using X3UR.UI.ViewModels.UserSettings.SettingsTab;

namespace X3UR.UI.ViewModels;
public class MainWindowViewModel {
    public SeedViewModel SeedVm { get; }
    public UniverseSettingsTabViewModel SettingsVm { get; }

    public ICommand GenerateCommand { get; }

    public MainWindowViewModel(SeedViewModel seedVm, UniverseSettingsTabViewModel settingsVm) {
        SeedVm = seedVm;
        SettingsVm = settingsVm;
        GenerateCommand = new RelayCommand(OnGenerate);
    }

    private void OnGenerate() {
        
    }
}