using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using System.Runtime.CompilerServices;
using Xunit;

namespace X3UR.UI.FlaUI.Tests.Helpers {
    public static class AutomationExtensions {
        /// <summary>
        /// Holt den n-ten Slider innerhalb einer Zeile (0‑basiert).
        /// </summary>
        public static Slider FindSlider(this AutomationElement row, int index) =>
            GetElementHelper(row, index, ControlType.Slider).AsSlider();

        /// <summary>
        /// Holt die n-te TextBox innerhalb einer Zeile (0‑basiert).
        /// </summary>
        public static TextBox FindTextBox(this AutomationElement row, int index) =>
            GetElementHelper(row, index, ControlType.Edit).AsTextBox();

        /// <summary>
        /// Holt die n-te Label innerhalb einer Zeile (0‑basiert).
        /// </summary>
        public static Label FindLabel(this AutomationElement row, int index) =>
            GetElementHelper(row, (index * 2), ControlType.Text).AsLabel();

        /// <summary>
        /// Holt die n-te CheckBox innerhalb einer Zeile (0‑basiert).
        /// </summary>
        public static CheckBox FindCheckBox(this AutomationElement row, int index) =>
            GetElementHelper(row, index, ControlType.CheckBox).AsCheckBox();

        /// <summary>
        /// Findet ein AutomationElement anhand seiner AutomationId.
        /// </summary>
        public static AutomationElement GetById(this Window window, string automationId) {
            var element = window.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element == null)
                throw new InvalidOperationException($"Element mit AutomationId='{automationId}' nicht gefunden");
            return element;
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

        /// <summary>
        /// Schreibt erst einen Wert unter min, wartet auf Update, liest den aktuellen Text aus und gibt ihn zurück,
        /// dann schreibt einen Wert über max, wartet nochmal und gibt den final eingestellten Wert zurück.
        /// </summary>
        public static (short lowClamped, short highClamped) ClampAndRead(this TextBox box, int min, int max) {
            box.Text = (min - 1).ToString();
            box.WaitUntilClickable();
            short low = short.Parse(box.Text);

            box.Text = (max + 1).ToString();
            box.WaitUntilClickable();
            short high = short.Parse(box.Text);

            return (low, high);
        }
        
        /// <summary>
         /// Liest alle Slider-Werte (CurrentSize, CurrentClusters, CurrentClusterSize) aus einer Zeile.
         /// </summary>
        public static int[] ReadAllSliderValues(this AutomationElement row) {
            var sliders = row
                .FindAllDescendants(cf => cf.ByControlType(ControlType.Slider))
                .Select(e => e.AsSlider().Value)
                .Select(v => (int)v)
                .ToArray();
            return sliders;
        }

        /// <summary>
        /// Liest alle TextBox-Werte (CurrentSize, CurrentClusters, CurrentClusterSize) aus einer Zeile.
        /// </summary>
        public static string[] ReadAllTextBoxValues(this AutomationElement row) {
            var boxes = row
                .FindAllDescendants(cf => cf.ByControlType(ControlType.Edit))
                .Select(e => e.AsTextBox().Text)
                .ToArray();
            return boxes;
        }

        private static AutomationElement GetElementHelper(AutomationElement row, int index, ControlType type) {
            var element = row.FindAllDescendants(cf => cf.ByControlType(type));
            if (index < 0 || index >= element.Length)
                throw new IndexOutOfRangeException($"Kein {type} an Index {index} gefunden");
            return element[index];
        }
    }
}