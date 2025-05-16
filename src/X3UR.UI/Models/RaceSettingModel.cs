using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using X3UR.Domain.Enums;

namespace X3UR.UI.Models {
    public class RaceSettingModel : INotifyPropertyChanged {
        public RaceNames Name { get; set; }
        public Brush Color { get; set; }

        private short _currentSizeSaved;
        private short _currentClustersSaved;
        private short _currentClusterSizeSaved;

        private bool _isActive;
        public bool IsActive {
            get => _isActive;
            set {
                if (_isActive == value) return;
                _isActive = value;
                OnPropertyChanged();

                if (!value) {
                    _currentSizeSaved = CurrentSize;
                    _currentClustersSaved = CurrentClusters;
                    _currentClusterSizeSaved = CurrentClusterSize;

                    CurrentSize = 0;
                    CurrentClusters = 0;
                    CurrentClusterSize = 0;
                } else {
                    CurrentSize = _currentSizeSaved;
                    CurrentClusters = _currentClustersSaved;
                    CurrentClusterSize = _currentClusterSizeSaved;
                }

            }
        }

        // aktuelle Größe
        private short _currentSize;
        public short CurrentSize {
            get => _currentSize;
            set {
                short old = _currentSize;

                _currentSize = value;
                OnPropertyChanged();

                // 1) Wenn wir gerade von 0 auf >0 wechseln, initialisiere Cluster & ClusterSize
                if (old == 0 && _currentSize > 0) {
                    CurrentClusters = 1;
                    CurrentClusterSize = 1;
                }
                else if (CurrentClusters > _currentSize) {
                    CurrentClusters = _currentSize;
                }

                // 2) Cluster-Size-Grenzen neu anpassen
                OnPropertyChanged(nameof(MinClusters));
                OnPropertyChanged(nameof(MaxClusters));

                // 3) Prozente neu berechnen
                UpdateDerived(_totalSectors);
            }
        }

        // neues Slider-Maximum für CurrentSize
        private short _maxSize;
        public short MaxSize {
            get => _maxSize;
            internal set {
                _maxSize = value;
                OnPropertyChanged();
            }
        }

        // Cluster-Grenzen
        public short MinClusters => (short)(CurrentSize > 0 ? 1 : 0);
        public short MaxClusters => CurrentSize;

        private short _currentClusters;
        public short CurrentClusters {
            get => _currentClusters;
            set {
                short min = MinClusters;
                short max = MaxClusters;

                _currentClusters = Math.Max(min, Math.Min(max, value));
                OnPropertyChanged();

                // Cluster-Size-Grenzen anpassen
                OnPropertyChanged(nameof(MinClusterSize));
                OnPropertyChanged(nameof(MaxClusterSize));

                // Prozente neu berechnen
                UpdateDerived(_totalSectors);

                if (CurrentClusterSize > MaxClusterSize)
                    CurrentClusterSize = MaxClusterSize;
                if (CurrentClusterSize > MaxClusterSize)
                    CurrentClusterSize = MaxClusterSize;
            }
        }

        public short MinClusterSize => (short)(CurrentSize > 0 ? 1 : 0);
        public short MaxClusterSize {
            get {
                if (CurrentSize == 0)
                    return 0;
                return (short)Math.Max(0, CurrentSize - CurrentClusters + 1);
            }
        }

        private short _currentClusterSize;
        public short CurrentClusterSize {
            get => _currentClusterSize;
            set {
                short min = MinClusterSize;
                short max = MaxClusterSize;

                _currentClusterSize = Math.Max(min, Math.Min(max, value));
                OnPropertyChanged();
            }
        }

        // Prozent-Anteil
        private float _sizePercentage;
        public float SizePercentage {
            get => _sizePercentage;
            private set { _sizePercentage = value; OnPropertyChanged(); }
        }

        // Zwischenspeicher für TotalSectors
        private short _totalSectors;
        public void UpdateDerived(short totalSectors) {
            _totalSectors = totalSectors;
            SizePercentage = totalSectors > 0 ? (float)CurrentSize / totalSectors : 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}