using System.Windows;
using X3UR.UI.ViewModels;
using X3UR.UI.Views.UserControls.VisualUniverse;

namespace X3UR.UI;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
        if (DataContext is MainWindowViewModel vm) {
            vm.UniverseCreated += universe => Dispatcher.Invoke(() => VisualUniverse.AttachUniverse(universe));
        }
    }
}