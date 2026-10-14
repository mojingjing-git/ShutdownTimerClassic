using System;
using System.Windows.Forms;

namespace ShutdownTimer
{
    /// <summary>
    /// A combo box entry that keeps its original English value intact while displaying a
    /// localized (Chinese) label to the user.
    ///
    /// WHY THIS EXISTS
    /// ---------------
    /// In the original application the combo box <c>Text</c> doubles as the application's
    /// internal identifier:
    ///   * <see cref="Timer.ExecutePowerAction"/> switch-matches on the literal English
    ///     strings "Shutdown" / "Restart" / "Hibernate" / "Sleep" / "Logout" / "Lock" /
    ///     "Custom Command";
    ///   * <see cref="Countdown"/> compares <c>TrayIconTheme</c> against "Light" / "Dark";
    ///   * <see cref="Menu"/>/<see cref="Settings"/> persist <c>actionComboBox.Text</c>
    ///     straight into the JSON settings file;
    ///   * command line arguments are parsed as English action names.
    ///
    /// Simply translating the combo box entries would therefore break the power action
    /// dispatch, break previously saved settings and break the CLI. By overriding
    /// <see cref="ToString"/> the UI shows Chinese text while every existing comparison,
    /// <c>switch</c> case and stored value keeps working unchanged.
    /// </summary>
    public class LocalizedItem
    {
        /// <summary>The stable, English, machine-readable value (never translated).</summary>
        public string Value { get; }

        /// <summary>The user-facing label shown in the combo box drop-down.</summary>
        public string Display { get; }

        public LocalizedItem(string value, string display)
        {
            Value = value;
            Display = display;
        }

        /// <summary>WinForms renders items by calling ToString(); return the localized label.</summary>
        public override string ToString()
        {
            return Display;
        }
    }

    /// <summary>
    /// Central catalog mapping the original English identifiers to their Chinese labels,
    /// plus helpers that read/write the underlying English value on a combo box.
    /// </summary>
    public static class LocalizedCatalog
    {
        /// <summary>Power actions, in the original drop-down order.</summary>
        public static readonly LocalizedItem[] PowerActions =
        {
            new LocalizedItem("Shutdown",        "关机"),
            new LocalizedItem("Restart",         "重启"),
            new LocalizedItem("Hibernate",       "休眠"),
            new LocalizedItem("Sleep",           "睡眠"),
            new LocalizedItem("Logout",          "注销"),
            new LocalizedItem("Lock",            "锁定"),
            new LocalizedItem("Custom Command",  "自定义命令")
        };

        /// <summary>Tray icon themes, in the original drop-down order.</summary>
        public static readonly LocalizedItem[] TrayIconThemes =
        {
            new LocalizedItem("Automatic", "自动"),
            new LocalizedItem("Light",     "浅色"),
            new LocalizedItem("Dark",      "深色")
        };

        /// <summary>
        /// Fills a combo box with localized items and selects the entry matching
        /// <paramref name="defaultValue"/>. Falls back to the first entry if the value is
        /// unknown (for example a hand-edited or corrupt settings file).
        /// </summary>
        public static void Fill(ComboBox box, LocalizedItem[] items, string defaultValue)
        {
            box.Items.Clear();

            // AutoComplete would otherwise match against the English values rather than
            // the Chinese labels the user actually sees.
            box.AutoCompleteMode = AutoCompleteMode.None;
            box.AutoCompleteSource = AutoCompleteSource.ListItems;

            foreach (LocalizedItem item in items)
            {
                box.Items.Add(item);
            }

            SelectValue(box, defaultValue);
        }

        /// <summary>
        /// Reads the English internal value from the combo box, regardless of whether the
        /// combo box was filled through <see cref="Fill"/> or is still in its original
        /// state (which keeps this method safe during a rolling migration).
        /// </summary>
        public static string GetValue(ComboBox box)
        {
            if (box == null)
            {
                return null;
            }

            if (box.SelectedItem is LocalizedItem localized)
            {
                return localized.Value;
            }

            return box.Text;
        }

        /// <summary>
        /// Selects the entry whose English <see cref="LocalizedItem.Value"/> equals
        /// <paramref name="value"/>. Falls back to the first entry when there is no match.
        /// </summary>
        public static void SelectValue(ComboBox box, string value)
        {
            if (box == null || box.Items.Count == 0)
            {
                return;
            }

            foreach (object raw in box.Items)
            {
                if (raw is LocalizedItem localized)
                {
                    if (string.Equals(localized.Value, value, System.StringComparison.Ordinal))
                    {
                        box.SelectedItem = localized;
                        return;
                    }
                }
                else if (string.Equals(Convert.ToString(raw), value, System.StringComparison.Ordinal))
                {
                    box.SelectedItem = raw;
                    return;
                }
            }

            box.SelectedIndex = 0;
        }

        /// <summary>Returns the Chinese label for an English value, or the value itself if unknown.</summary>
        public static string DisplayFor(string value, LocalizedItem[] items)
        {
            foreach (LocalizedItem item in items)
            {
                if (string.Equals(item.Value, value, System.StringComparison.Ordinal))
                {
                    return item.Display;
                }
            }

            return value;
        }
    }
}
