using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using X3UR.UI.Bootstrap;
using X3UR.UI.ViewModels;

namespace X3UR.UI;
public partial class App : System.Windows.Application {
    protected override void OnStartup(StartupEventArgs e) {
        Bootstrapper.Initialize();

        var mainVm = Bootstrapper.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        var mainWindow = new MainWindow {
            DataContext = mainVm
        };

        mainWindow.Show();
        base.OnStartup(e);
    }
}