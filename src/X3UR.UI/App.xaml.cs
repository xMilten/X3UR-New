using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using X3UR.UI.Bootstrap;

namespace X3UR.UI;
public partial class App : System.Windows.Application {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        Bootstrapper.Initialize();

        var sp = Bootstrapper.ServiceProvider;
        var mainWindow = sp.GetRequiredService<MainWindow>();

        mainWindow.Show();
    }
}