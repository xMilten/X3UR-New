using System.ComponentModel;

namespace X3UR.Domain.Interfaces;
public interface ISeedProvider : INotifyPropertyChanged {
    long Seed { get; set; }
}