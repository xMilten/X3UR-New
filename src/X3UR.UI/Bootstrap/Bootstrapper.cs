using Microsoft.Extensions.DependencyInjection;
using X3UR.UI.ViewModels.UserSettings.SettingsTab;
using X3UR.UI.ViewModels;

namespace X3UR.UI.Bootstrap;
public static class Bootstrapper {
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void Initialize() {
        var services = new ServiceCollection();

        // 1) Domain- und Application‑Services registrieren:
        //services.AddSingleton<IRandomProvider, RandomProvider>();
        //services.AddSingleton<IUniverseGenerator, GrowingUniverseGenerator>();

        // 2) ViewModels registrieren
        services.AddTransient<UniverseSettingsTabViewModel>();
        services.AddTransient<MainWindowViewModel>();

        // 3) Views registrieren
        // services.AddTransient<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();
    }
}