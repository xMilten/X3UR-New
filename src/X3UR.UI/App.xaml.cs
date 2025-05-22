using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using X3UR.UI.Bootstrap;

namespace X3UR.UI;
public partial class App : System.Windows.Application {
    protected override void OnStartup(StartupEventArgs e) {
        Bootstrapper.Initialize();
        var mw = Bootstrapper.ServiceProvider.GetRequiredService<MainWindow>();
        mw.Show();
        base.OnStartup(e);
    }
}