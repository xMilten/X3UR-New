using System;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;

namespace X3UR.UI.FlaUI.Tests.Helpers {
    public static class AutomationExtensions {
        private static readonly Dictionary<string, ControlType> namedControlTypes = new() {
            {"Slider", ControlType.Slider},
            {"TextBox", ControlType.Edit}
        };

        /// <summary>
        /// Holt den n-ten Slider innerhalb einer Zeile (0‑basiert).
        /// </summary>
        public static Slider FindSlider(this AutomationElement row, int index) {
            return GetElementHelper(row, index, "Slider").AsSlider();
        }

        /// <summary>
        /// Holt die n-te TextBox innerhalb einer Zeile (0‑basiert).
        /// </summary>
        public static TextBox FindTextBox(this AutomationElement row, int index) {
            return GetElementHelper(row, index, "TextBox").AsTextBox();
        }

        private static AutomationElement GetElementHelper(this AutomationElement row, int index, string elementName) {
            var element = row.FindAllDescendants(cf => cf.ByControlType(namedControlTypes[elementName]));
            if (index < 0 || index >= element.Length)
                throw new IndexOutOfRangeException($"Kein {elementName} an Index {index} gefunden");
            return element[index];
        }

        /// <summary>
        /// Wartet, bis das Element klickbar ist (also gerendert und enabled).
        /// </summary>
        public static void EnsureClickable(this AutomationElement element, TimeSpan? timeout = null) {
            var to = timeout ?? TimeSpan.FromSeconds(2);
            var end = DateTime.Now + to;
            while (DateTime.Now < end) {
                if (element.IsEnabled)
                    return;
                Thread.Sleep(50);
            }
            throw new TimeoutException($"{element.Name}-Element nicht innerhalb von Frist klickbar.");
        }

        /// <summary>
        /// Aktiviert einen Tab mit gegebener AutomationId und Headertext.
        /// </summary>
        public static void ActivateTab(this Window window, string automationId, string tabHeader) {
            var tabEl = window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (tabEl == null)
                throw new InvalidOperationException($"TabControl '{automationId}' nicht gefunden");
            var tab = tabEl.AsTab();
            tab.SelectTabItem(tabHeader);
        }

        /// <summary>
        /// Holt alle DataItem‑Zeilen unterhalb eines ItemsControl mit gegebener AutomationId.
        /// </summary>
        public static AutomationElement[] GetDataItems(this Window window, string itemsControlId) {
            var ctrl = window.FindFirstDescendant(cf => cf.ByAutomationId(itemsControlId));
            if (ctrl == null)
                throw new InvalidOperationException($"ItemsControl '{itemsControlId}' nicht gefunden");
            return ctrl.FindAllDescendants(cf => cf.ByControlType(ControlType.DataItem));
        }
    }
}