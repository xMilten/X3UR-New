using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using X3UR.Domain.Enums;

namespace X3UR.UI.Models;
public class RaceSettingModel : INotifyPropertyChanged {
    public RaceNames Name { get; set; }
    public Brush Color { get; set; }

    private bool _isActive;
    public bool IsActive {
        get => _isActive;
        set {
            _isActive = value;
            OnPropertyChanged();
        }
    }

    private int _maxSize;
    public int MaxSize {
        get => _maxSize;
        set {
            _maxSize = value;
            OnPropertyChanged();
            UpdateDerived();
        }
    }

    private int _maxClusters;
    public int MaxClusters {
        get => _maxClusters;
        set {
            _maxClusters = value;
            OnPropertyChanged();
            UpdateDerived();
        }
    }

    private int _maxClusterSize;
    public int MaxClusterSize {
        get => _maxClusterSize;
        set {
            _maxClusterSize = value;
            OnPropertyChanged();
            UpdateDerived();
        }
    }

    // Für Prozent-Anteile
    public double SizePercentage { get; private set; }
    public void UpdateDerived() {
        // Beispiel: berechne anhand Gesamtsize im Parent-VM später
        OnPropertyChanged(nameof(SizePercentage));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}