using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using X3UR.Domain.DTOs;
using X3UR.UI.Models;

namespace X3UR.UI.ViewModels.UserSettings.SettingsTab {
    public class UniverseSettingsTabViewModel : INotifyPropertyChanged {
        private byte _width = 22;
        public byte Width {
            get => _width;
            set {
                byte min = MinWidth;
                byte max = MaxWidth;
                _width = Math.Max(min, Math.Min(max, value));

                OnPropertyChanged();
                UpdateTotal();
            }
        }

        private byte _height = 17;
        public byte Height {
            get => _height;
            set {
                byte min = MinHeight;
                byte max = MaxHeight;
                _height = Math.Max(min, Math.Min(max, value));

                OnPropertyChanged();
                UpdateTotal();
            }
        }

        public byte MinWidth { get; } = 5;
        public byte MinHeight { get; } = 5;
        public byte MaxWidth { get; } = 24;
        public byte MaxHeight { get; } = 20;

        public short TotalSectorCount => (short)(Width * Height);

        // Summe aller vergebenen Sektoren
        public short TotalRaceSize => (short)RaceSettings.Sum(r => r.CurrentSize);
        public float RaceSizePercentage =>
            TotalSectorCount > 0 ? (float)TotalRaceSize / TotalSectorCount : 0;

        public ObservableCollection<RaceSettingModel> RaceSettings { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public UniverseSettingsTabViewModel() {
            RaceSettings = [.. RaceDefinitions.All.Select(definition => {
                RaceSettingModel model = new() { Name = definition.Name,
                    Color = new SolidColorBrush(definition.Color),
                    IsActive = definition.IsDefaultActive,
                    CurrentSize = definition.DefaultSize,
                    CurrentClusters = definition.DefaultClusters,
                    CurrentClusterSize = definition.DefaultClusterSize
                };
                model.PropertyChanged += RaceModel_PropertyChanged;
                return model;
            })];

            RaceSettings.CollectionChanged += (s, e) => UpdateAllDerived();
            UpdateAllDerived();
        }

        private void RaceModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(RaceSettingModel.CurrentSize) ||
                e.PropertyName == nameof(RaceSettingModel.CurrentClusters)) {
                UpdateTotal();
            }
        }

        private void UpdateTotal() {
            OnPropertyChanged(nameof(TotalSectorCount));
            OnPropertyChanged(nameof(TotalRaceSize));
            OnPropertyChanged(nameof(RaceSizePercentage));
            UpdateAllDerived();
        }

        private void UpdateAllDerived() {
            short total = TotalSectorCount;
            // 1) Prozent-Anteile aktualisieren
            foreach (RaceSettingModel race in RaceSettings)
                race.UpdateDerived(total);

            // 2) Rest-Kontingent berechnen
            short used = (short)RaceSettings.Sum(r => r.CurrentSize);
            short remaining = (short)Math.Max(0, total - used);

            // 3) MaxSize für jeden Slider setzen
            foreach (RaceSettingModel race in RaceSettings) {
                race.MaxSize = (short)(race.CurrentSize + remaining);
            }
        }

        public UniverseSettingsDto ToDto() {
            return new UniverseSettingsDto {
                Width = this.Width,
                Height = this.Height,
                RaceSettings = RaceSettings
                .Select(r => new RaceSettingDto {
                    Name = r.Name,
                    ColorHex = (r.Color as SolidColorBrush)?.Color.ToString() ?? "#FFFFFF",
                    CurrentSize = r.CurrentSize,
                    CurrentClusters = r.CurrentClusters,
                    CurrentClusterSize = r.CurrentClusterSize,
                    IsActive = r.IsActive
                })
                .ToList()
            };
        }
    }
}