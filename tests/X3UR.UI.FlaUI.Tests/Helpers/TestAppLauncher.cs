using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X3UR.UI.FlaUI.Tests.Helpers;
public static class TestAppLauncher {
    private const string ExeRelativePath = @"..\..\..\..\..\src\X3UR.UI\bin\Debug\net8.0-windows7.0\X3UR.UI.exe";

    /// <summary>
    /// Liefert den absoluten Pfad zur UI-Exe, basierend auf dem aktuellen Arbeitsverzeichnis.
    /// </summary>
    public static string GetExePath() {
        var baseDir = Environment.CurrentDirectory;
        return Path.GetFullPath(Path.Combine(baseDir, ExeRelativePath));
    }
}