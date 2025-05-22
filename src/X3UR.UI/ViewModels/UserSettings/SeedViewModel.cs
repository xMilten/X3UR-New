using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using X3UR.Domain.Interfaces;
using X3UR.Infrastructure.Commands;

namespace X3UR.UI.ViewModels.UserSettings;
public class SeedViewModel : INotifyPropertyChanged {
    private readonly ISeedProvider _seedProvider;

    public string Seed {
        get => _seedProvider.Seed.ToString();
        set {
            if (long.TryParse(value, out var seed)) {
                _seedProvider.Seed = seed;
                OnPropertyChanged();
            }
        }
    }

    public ICommand GenerateSeedCommand { get; }

    public SeedViewModel(ISeedProvider seedProvider) {
        Random rnd = new();
        _seedProvider = seedProvider;
        GenerateSeedCommand = new RelayCommand(() => {
            _seedProvider.Seed = ((long)rnd.Next() << 32) | (uint)rnd.Next();
            OnPropertyChanged(nameof(Seed));
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}