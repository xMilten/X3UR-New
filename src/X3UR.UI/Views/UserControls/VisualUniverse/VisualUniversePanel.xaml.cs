using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using X3UR.Domain.DTOs;
using X3UR.Domain.Enums;
using X3UR.Domain.Models;
using X3UR.UI.Models;

namespace X3UR.UI.Views.UserControls.VisualUniverse;
/// <summary>
/// Interaktionslogik für VisualUniversePanel.xaml
/// </summary>
public partial class VisualUniversePanel : UserControl {
    private Universe? _universe;
    private readonly ConcurrentDictionary<(byte x, byte y), FrameworkElement> _elements = new();
    private BitmapImage? _icon;

    public ObservableCollection<SectorVisual> SectorVisuals { get; } = new();

    public VisualUniversePanel() {
        InitializeComponent();
        SectorVisuals.CollectionChanged += SectorVisuals_CollectionChanged;
        Loaded += VisualUniversePanel_Loaded;
    }

    private void VisualUniversePanel_Loaded(object? sender, RoutedEventArgs e) {
        TryLoadIcon();
        LayoutSectors();
    }

    private void TryLoadIcon() {
        try {
            _icon = new BitmapImage(new Uri("pack://application:,,,/sectorIcon.png", UriKind.Absolute));
        } catch {
            _icon = null;
        }
    }

    // Muss VOR GenerateAsync aufgerufen werden
    public void AttachUniverse(Universe universe) {
        _universe = universe ?? throw new ArgumentNullException(nameof(universe));

        // Abonniere ClusterAdded damit wir neue Cluster sofort sehen
        _universe.ClusterAdded += OnUniverseClusterAdded;

        // Falls bereits Cluster existieren (z. B. nach Fertigstellung), visualisieren
        foreach (var cluster in _universe.Clusters) {
            SubscribeCluster(cluster);
            foreach (var s in cluster.GetSectors())
                AddSectorVisualForSector(s);
        }
    }

    private void OnUniverseClusterAdded(Cluster cluster) {
        SubscribeCluster(cluster);
        // Der Generator fügt typischerweise sofort einen Sector hinzu; falls vorhanden, abonniere und warte auf SectorAdded.
        foreach (var s in cluster.GetSectors())
            AddSectorVisualForSector(s);
    }

    private void SubscribeCluster(Cluster cluster) {
        cluster.SectorAdded += OnClusterSectorAdded;
    }

    private void OnClusterSectorAdded(Sector sector) {
        Dispatcher.Invoke(() => AddSectorVisualForSector(sector));
    }

    private void AddSectorVisualForSector(Sector sector) {
        if (sector == null)
            return;
        var brush = TryCreateBrushFromHex(sector.Cluster?.Race?.ColorHex) ?? Brushes.Gray;
        var sv = new SectorVisual { X = sector.X, Y = sector.Y, Race = sector.Race, Fill = brush };
        SectorVisuals.Add(sv);
    }

    private Brush? TryCreateBrushFromHex(string? hex) {
        if (string.IsNullOrWhiteSpace(hex))
            return null;
        try {
            var c = (Color)ColorConverter.ConvertFromString(hex);
            return new SolidColorBrush(c);
        } catch {
            return null;
        }
    }

    private void SectorVisuals_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.NewItems != null) {
            foreach (SectorVisual sv in e.NewItems.Cast<SectorVisual>()) {
                var key = (sv.X, sv.Y);
                if (_elements.ContainsKey(key))
                    continue;

                var element = CreateElementForSectorVisual(sv);
                _elements[key] = element;
                RootCanvas.Children.Add(element);
            }
        }

        if (e.OldItems != null) {
            foreach (SectorVisual sv in e.OldItems.Cast<SectorVisual>()) {
                var key = (sv.X, sv.Y);
                if (_elements.TryRemove(key, out var el)) {
                    RootCanvas.Children.Remove(el);
                }
            }
        }

        LayoutSectors();
    }

    private FrameworkElement CreateElementForSectorVisual(SectorVisual sv) {
        var grid = new Grid { Tag = sv };
        var rect = new Rectangle { Fill = sv.Fill, RadiusX = 2, RadiusY = 2 };
        grid.Children.Add(rect);

        if (_icon != null) {
            var img = new Image { Source = _icon, Stretch = Stretch.Uniform, Opacity = 0.95, IsHitTestVisible = false };
            grid.Children.Add(img);
        }

        ToolTipService.SetToolTip(grid, sv.Race.ToString());
        return grid;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) {
        LayoutSectors();
    }

    private void LayoutSectors() {
        if (_universe == null) {
            LayoutUsingSectorVisuals();
            return;
        }

        if (_universe.Width <= 0 || _universe.Height <= 0)
            return;

        double totalW = Math.Max(0.0, ActualWidth);
        double totalH = Math.Max(0.0, ActualHeight);

        int cols = _universe.Width;
        int rows = _universe.Height;

        double cellW = totalW / cols;
        double cellH = totalH / rows;
        double cellSize = Math.Min(cellW, cellH);

        double spacing = cellSize * 0.10;
        double imageSize = Math.Max(0.0, cellSize - spacing);

        foreach (var kv in _elements) {
            byte x = kv.Key.x;
            byte y = kv.Key.y;
            var el = kv.Value;

            double originX = x * cellW;
            double originY = y * cellH;

            double left = originX + (cellW - imageSize) / 2.0;
            double top = originY + (cellH - imageSize) / 2.0;

            el.Width = imageSize;
            el.Height = imageSize;

            Canvas.SetLeft(el, left);
            Canvas.SetTop(el, top);
        }
    }

    private void LayoutUsingSectorVisuals() {
        if (SectorVisuals.Count == 0)
            return;

        int maxX = SectorVisuals.Max(s => s.X) + 1;
        int maxY = SectorVisuals.Max(s => s.Y) + 1;

        double totalW = Math.Max(0.0, ActualWidth);
        double totalH = Math.Max(0.0, ActualHeight);

        int cols = Math.Max(1, maxX);
        int rows = Math.Max(1, maxY);

        double cellW = totalW / cols;
        double cellH = totalH / rows;
        double cellSize = Math.Min(cellW, cellH);

        double spacing = cellSize * 0.10;
        double imageSize = Math.Max(0.0, cellSize - spacing);

        foreach (var kv in _elements) {
            byte x = kv.Key.x;
            byte y = kv.Key.y;
            var el = kv.Value;

            double originX = x * cellW;
            double originY = y * cellH;

            double left = originX + (cellW - imageSize) / 2.0;
            double top = originY + (cellH - imageSize) / 2.0;

            el.Width = imageSize;
            el.Height = imageSize;

            Canvas.SetLeft(el, left);
            Canvas.SetTop(el, top);
        }
    }
}
