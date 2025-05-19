using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace X3UR.UI.Controls;
public class ExposedStackPanel : StackPanel {
    protected override AutomationPeer OnCreateAutomationPeer()
        => new FrameworkElementAutomationPeer(this);
}