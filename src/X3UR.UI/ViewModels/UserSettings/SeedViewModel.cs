using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using X3UR.Domain.Interfaces;
using X3UR.Infrastructure.Commands;

namespace X3UR.UI.ViewModels.UserSettings;
public class SeedViewModel : INotifyPropertyChanged {
    private string _seed;
    public string Seed {
        get => _seed;
        set {
            if (_seed != value) {
                _seed = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand GenerateSeedCommand { get; }

    public SeedViewModel(ISeedProvider seedProvider) {
        Seed = seedProvider.GetSeed().ToString();
        GenerateSeedCommand = new RelayCommand(() => {
            Seed = seedProvider.GetSeed().ToString();
        });
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}