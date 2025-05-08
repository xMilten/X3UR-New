using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using X3UR.UI.FlaUI.Tests.Helpers;
using Xunit;
using FlauiApp = FlaUI.Core.Application;

namespace X3UR.UI.FlaUI.Tests;
public class UniverseSettingsTabFlaUITests : IDisposable {
    private readonly FlauiApp _app;
    private readonly AutomationBase _automation;
    private readonly Window _mainWindow;

    private const string ExeRelativePath = @"..\..\..\..\..\src\X3UR.UI\bin\Debug\net8.0-windows7.0\X3UR.UI.exe";

    public UniverseSettingsTabFlaUITests() {
        _app = FlauiApp.Launch(GetExePath());
        _automation = new UIA3Automation();
        _mainWindow = _app.GetMainWindow(_automation);

        // Sicherstellen, dass der „Universum“-Tab aktiv ist
        _mainWindow.ActivateTab("SettingsTab", "Universum");
    }

    private static string GetExePath() =>
        Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, ExeRelativePath));

    public void Dispose() {
        _automation.Dispose();
        _app.Close();
    }

    // Hilfsmethode: findet den ItemsControl für RaceSettings
    private AutomationElement[] GetRaceRows() {
        var rows = _mainWindow.GetDataItems("RaceSettingsItemsControl");
        return rows;
    }

    [Fact]
    public void SizeSlider_ShouldHaveCorrectMinMax() {
        var firstRow = GetRaceRows()[0];
        var slider = firstRow.FindSlider(0);

        Assert.Equal(0, slider.Minimum);

        // Maximum = CurrentSize + remaining
        // Lese initial CurrentSize aus TextBox
        var sizeBox = firstRow.FindTextBox(0);
        short initialSize = short.Parse(sizeBox.Text);

        // Lese Label TotalSectorCount
        var totalLabel = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("TotalSectorCountLabel")).AsLabel();
        short total = short.Parse(totalLabel.Text);

        // Sum initial Sizes
        firstRow.EnsureClickable();
        short sumSizes = 0;
        Console.WriteLine($"RaceRows: {GetRaceRows().Length}");
        foreach (var row in GetRaceRows()) {
            var txt = row.FindTextBox(0);
            sumSizes += short.Parse(txt.Text);
        }
        short remaining = (short)(total - sumSizes + initialSize);

        Assert.Equal(remaining, slider.Maximum);
    }

    [Fact]
    public void SettingCurrentSizeToZero_ResetsClustersAndClusterSize() {
        var firstRow = GetRaceRows()[0];
        var sizeBox = firstRow.FindTextBox(0);
        sizeBox.Text = "0";
        firstRow.EnsureClickable();

        // Finde ClusterCount Slider und ClusterSize Slider
        var clustersSlider = firstRow.FindSlider(1);
        var clusterSizeSlider = firstRow.FindSlider(2);

        Assert.Equal(0, clustersSlider.Value);
        Assert.Equal(0, clusterSizeSlider.Value);
    }

    [Fact]
    public void IncreasingSizeFromZero_InitializesClustersAndClusterSizeToOne() {
        var firstRow = GetRaceRows()[0];
        var sizeBox = firstRow.FindTextBox(0);

        // Aktiviere erst 0
        sizeBox.Text = "0";
        firstRow.EnsureClickable();

        // Dann auf 5
        sizeBox.Text = "5";
        firstRow.EnsureClickable();

        var clusterSlider = firstRow.FindSlider(1);
        var clusterSizeSlider = firstRow.FindSlider(2);

        Assert.Equal(1, clusterSlider.Value);
        Assert.Equal(1, clusterSizeSlider.Value);
    }
}