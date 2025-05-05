using System.Windows.Input;
using X3UR.Infrastructure.Commands;
using X3UR.UI.ViewModels.UserSettings.SettingsTab;

namespace X3UR.UI.ViewModels;
public class MainWindowViewModel {
    public UniverseSettingsTabViewModel SettingsVm { get; }

    public ICommand GenerateCommand { get; }

    public MainWindowViewModel(UniverseSettingsTabViewModel settingsVm) {
        SettingsVm = settingsVm;
        GenerateCommand = new RelayCommand(OnGenerate);
    }

    private void OnGenerate() {
        // Hier später: Aufruf deines Generators mit SettingsVm.Width, SettingsVm.Height, SettingsVm.RaceSettings…
    }
}