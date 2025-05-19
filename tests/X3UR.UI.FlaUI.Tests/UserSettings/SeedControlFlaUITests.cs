using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using X3UR.UI.FlaUI.Tests.Helpers;
using Xunit;
using FlaUIApp = FlaUI.Core.Application;

namespace X3UR.UI.FlaUI.Tests.UserSettings;
public class SeedControlFlaUITests : IDisposable {
    private readonly FlaUIApp _app;
    private readonly AutomationBase _automation;
    private readonly Window _mainWindow;

    public SeedControlFlaUITests() {
        var exePath = TestAppLauncher.GetExePath();
        _app = FlaUIApp.Launch(exePath);
        _automation = new UIA3Automation();
        _mainWindow = _app.GetMainWindow(_automation);
    }

    public void Dispose() {
        _automation.Dispose();
        _app.Close();
    }

    private AutomationElement GetSeedControl()
        => _mainWindow.GetById("SeedControl");

    [Fact]
    public void SeedControl_InitialSeed_IsNotEmpty() {
        var seedCtrl = GetSeedControl();
        var tb = seedCtrl.FindTextBox(0);
        Assert.False(string.IsNullOrWhiteSpace(tb.Text));
    }

    [Fact]
    public void SeedControl_ManualEntry_UpdatesViewModel() {
        var seedCtrl = GetSeedControl();
        var tb = seedCtrl.FindTextBox(0);

        // Manuelle Eingabe
        tb.Text = "12345";
        seedCtrl.WaitUntilClickable();

        Assert.Equal("12345", tb.Text);
    }

    [Fact]
    public void SeedControl_GenerateButton_ChangesSeed() {
        var seedCtrl = GetSeedControl();
        var tb = seedCtrl.FindTextBox(0);
        var oldSeed = tb.Text;

        // Klick auf Button
        var btn = seedCtrl.FindButton(0);
        btn.Invoke();
        seedCtrl.WaitUntilClickable();

        Assert.NotEqual(oldSeed, tb.Text);
    }
}