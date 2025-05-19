using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using X3UR.UI.FlaUI.Tests.Helpers;
using Xunit;

using FlauiApp = FlaUI.Core.Application;

namespace X3UR.UI.FlaUI.Tests.UserSettings.SettingsTabs;
public class UniverseSettingsTabFlaUITests : IDisposable {
    private readonly FlauiApp _app;
    private readonly AutomationBase _automation;
    private readonly Window _mainWindow;

    public UniverseSettingsTabFlaUITests() {
        var exePath = TestAppLauncher.GetExePath();
        _app = FlauiApp.Launch(exePath);
        _automation = new UIA3Automation();
        _mainWindow = _app.GetMainWindow(_automation);
        // Sicherstellen, dass der „Universum“-Tab aktiv ist
        _mainWindow.ActivateTab("SettingsTab", "Universum");
    }

    public void Dispose() {
        _automation.Dispose();
        _app.Close();
    }

    private AutomationElement[] GetRaceRows() {
        var rows = _mainWindow.GetDataItems("RaceSettingsItemsControl");
        return rows;
    }

    [Fact]
    public void MapSettings_AreCorrectlyInitializedAndBound() {
        var mapGrid = _mainWindow.GetById("MapSettingsGrid");
        var totalLabel = mapGrid.FindLabel(2);
        var widthSlider = mapGrid.FindSlider(0);
        var widthBox = mapGrid.FindTextBox(0);
        var heightSlider = mapGrid.FindSlider(1);
        var heightBox = mapGrid.FindTextBox(1);

        byte width = byte.Parse(widthBox.Text);
        byte height = byte.Parse(heightBox.Text);

        // 1) TotalSectorCount = Width * Height
        Assert.Equal(width * height, short.Parse(totalLabel.Text));

        // 2) Slider und TextBox synchronisiert
        Assert.Equal(width, (byte)widthSlider.Value);
        Assert.Equal(height, (byte)heightSlider.Value);

        // 3) Eingabefelder clamped auf Min/Max
        var (widthLow, widthHigh) = widthBox.ClampAndRead(5, 24);
        var (heightLow, heightHigh) = heightBox.ClampAndRead(5, 20);

        Assert.Equal(5, widthLow);
        Assert.Equal(24, widthHigh);

        Assert.Equal(5, heightLow);
        Assert.Equal(20, heightHigh);
    }

    [Fact]
    public void RaceTable_Activation_Deactivation_WorksAndBoundsAreRespected() {
        var firstRow = GetRaceRows()[0];
        var chk = firstRow.FindCheckBox(0);

        // speichere Standard-Werte
        var defaultSliders = firstRow.ReadAllSliderValues();
        var defaultsTexts = firstRow.ReadAllTextBoxValues();

        // deaktivieren -> alle auf 0
        chk.IsChecked = false;
        firstRow.WaitUntilClickable();

        // auktuelle Werte auslesen
        var currentSliders = firstRow.ReadAllSliderValues();
        var currentTexts = firstRow.ReadAllTextBoxValues();

        // 1) aktuelle Werte auf 0 überprüfen
        Assert.All(currentSliders, v => Assert.Equal(0, v));
        Assert.All(currentTexts, t => Assert.Equal("0", t));

        // reaktivieren -> alle zurück auf Standard
        chk.IsChecked = true;
        firstRow.WaitUntilClickable();

        // wiederhergestellte Werte auslesen
        var restoredSliders = firstRow.ReadAllSliderValues();
        var restoredTexts = firstRow.ReadAllTextBoxValues();

        // 2) Standard-Werte mit wiederhergestellte Werte vergleichen
        Assert.Equal(defaultSliders, restoredSliders);
        Assert.Equal(defaultsTexts, restoredTexts);
    }

    [Fact]
    public void OverallStatistics_AreCalculatedCorrectly() {
        var mapGrid = _mainWindow.GetById("MapSettingsGrid");
        var totalSectorCount = mapGrid.FindLabel(2);
        int total = int.Parse(totalSectorCount.Text);
        int sum = GetRaceRows().Sum(r => int.Parse(r.FindTextBox(0).Text));
        var overallStats = _mainWindow.GetById("OverallStatistics");
        int totalRaceSize = int.Parse(overallStats.FindLabel(1).Text);
        float pct = float.Parse(overallStats.FindLabel(2).Text);

        // 1) Die Summe der Größe aller Rassen mit TotalSectorCount vergleichen
        Assert.Equal(sum, totalRaceSize);

        // 2) Die Summe der Größe aller Rassen als Faktor mit dem Faktor-Label vergleichen
        Assert.Equal((float)sum / total, pct, 2);
    }
}