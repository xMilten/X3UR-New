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
        set {
            _width = value;
            OnPropertyChanged();
            UpdateTotal();
        }
    }

    private int _height = 17;
    public int Height {
        get => _height;
        set {
            _height = value;
            OnPropertyChanged();
            UpdateTotal();
        }
    }

    public int MinWidth { get; } = 5;
    public int MinHeight { get; } = 5;
    public int MaxWidth { get; } = 24;
    public int MaxHeight { get; } = 20;

    public int TotalSectorCount => Width * Height;

    public int TotalRaceSize => RaceSettings.Sum(r => r.MaxSize);
    public double RaceSizePercentage => TotalSectorCount > 0 ? (double)TotalRaceSize / TotalSectorCount : 0;

    public ObservableCollection<RaceSettingModel> RaceSettings { get; }

    public UniverseSettingsTabViewModel() {
        RaceSettings = new ObservableCollection<RaceSettingModel>(
            RaceDefinitions.All.Select(definition => {
                RaceSettingModel model = new() {
                    Name = definition.Name,
                    Color = new SolidColorBrush(definition.Color),
                    MaxSize = definition.DefaultSize,
                    MaxClusters = definition.DefaultClusters,
                    MaxClusterSize = definition.DefaultClusterSize,
                    IsActive = definition.IsDefaultActive
                };

                model.PropertyChanged += RaceModel_PropertyChanged;
                return model;
            })
        );

        RaceSettings.CollectionChanged += (s, e) => UpdateAllDerived();
        UpdateAllDerived();
    }

    private void RaceModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        // Wenn MaxSize sich ändert, neu berechnen
        if (e.PropertyName == nameof(RaceSettingModel.MaxSize)) {
            var model = (RaceSettingModel)sender;
            model.UpdateDerived(TotalSectorCount);
            // Gesamtstatistik anpassen
            OnPropertyChanged(nameof(TotalRaceSize));
            OnPropertyChanged(nameof(RaceSizePercentage));
        }
    }

    private void UpdateTotal() {
        OnPropertyChanged(nameof(TotalSectorCount));
        OnPropertyChanged(nameof(TotalRaceSize));
        OnPropertyChanged(nameof(RaceSizePercentage));
        UpdateAllDerived();
    }

    private void UpdateAllDerived() {
        int total = TotalSectorCount;

        foreach (var race in RaceSettings)
            race.UpdateDerived(total);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}