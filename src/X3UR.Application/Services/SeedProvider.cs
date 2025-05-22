using System.ComponentModel;
using System.Runtime.CompilerServices;
using X3UR.Domain.Interfaces;

namespace X3UR.Application.Services;
public class SeedProvider : ISeedProvider {
    private long _seed;
    public long Seed {
        get => _seed;
        set {
            if (_seed == value)
                return;
            _seed = value;
            OnPropertyChanged();
        }
    }

    public SeedProvider() {
        Random rnd = new();
        _seed = ((long)rnd.Next() << 32) | (uint)rnd.Next();
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}