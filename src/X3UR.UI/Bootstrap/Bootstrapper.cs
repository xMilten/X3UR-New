using Microsoft.Extensions.DependencyInjection;
using X3UR.UI.ViewModels.UserSettings.SettingsTab;
using X3UR.UI.ViewModels;
using X3UR.Domain.Interfaces;
using X3UR.Infrastructure.Random;
using X3UR.UI.ViewModels.UserSettings;

namespace X3UR.UI.Bootstrap;
public static class Bootstrapper {
    public static IServiceProvider ServiceProvider { get; private set; }

    public static void Initialize() {
        var services = new ServiceCollection();

        // 1) Domain- und Application‑Services registrieren:
        services.AddSingleton<ISeedProvider, RandomSeedProvider>();
        //services.AddSingleton<IRandomProvider, RandomProvider>();
        //services.AddSingleton<IUniverseGenerator, GrowingUniverseGenerator>();

        // 2) ViewModels registrieren
        services.AddSingleton<SeedViewModel>();
        services.AddSingleton<UniverseSettingsTabViewModel>();
        services.AddSingleton<MainWindowViewModel>();

        // 3) Views registrieren
        services.AddSingleton<MainWindow>(sp => {
            var vm = sp.GetRequiredService<MainWindowViewModel>();
            return new MainWindow { DataContext = vm };
        });

        ServiceProvider = services.BuildServiceProvider();
    }
}