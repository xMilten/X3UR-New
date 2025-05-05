using System.Windows;
using X3UR.UI.ViewModels;
using X3UR.UI.ViewModels.UserSettings.SettingsTab;

namespace X3UR.UI;
public partial class App : System.Windows.Application {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);

        // Composition Root: nur die Instanzierung deines Settings-ViewModels
        var settingsVm = new UniverseSettingsTabViewModel();

        // MainWindowViewModel erhält das gerade erzeugte Settings-VM
        var mainVm = new MainWindowViewModel(settingsVm);

        // MainWindow bekommt sein ViewModel
        var mainWindow = new MainWindow {
            DataContext = mainVm
        };

        mainWindow.Show();
    }
}