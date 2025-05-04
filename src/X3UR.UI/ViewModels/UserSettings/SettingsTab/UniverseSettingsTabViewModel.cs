using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using X3UR.UI.Models;

namespace X3UR.UI.ViewModels.UserSettings.SettingsTab;
public class UniverseSettingsTabViewModel : INotifyPropertyChanged {
    private int _width = 22;
    public int Width {
        get => _width;
        set { _width = value; OnPropertyChanged(); UpdateTotal(); }
    }

    private int _height = 17;
    public int Height {
        get => _height;
        set { _height = value; OnPropertyChanged(); UpdateTotal(); }
    }

    public int MinWidth { get; } = 5;
    public int MinHeight { get; } = 5;
    public int MaxWidth { get; } = 24;
    public int MaxHeight { get; } = 20;

    public int TotalSectorCount => Width * Height;

    public int TotalRaceSize => RaceSettings.Sum(r => r.MaxSize);
    public double RaceSizePercentage =>
        TotalSectorCount > 0 ? (double)TotalRaceSize / TotalSectorCount : 0;

    public ObservableCollection<RaceSettingModel> RaceSettings { get; } = new(
        RaceDefinitions.All.Select(def => new RaceSettingModel {
            Name = def.Name,
            Color = new SolidColorBrush(def.Color),
            MaxSize = def.DefaultSize,
            MaxClusters = def.DefaultClusters,
            MaxClusterSize = def.DefaultClusterSize,
            IsActive = def.IsDefaultActive
        })
    );

    private void UpdateTotal() {
        OnPropertyChanged(nameof(TotalSectorCount));
        OnPropertyChanged(nameof(TotalRaceSize));
        OnPropertyChanged(nameof(RaceSizePercentage));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}