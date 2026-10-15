using ShutdownTimer.Helpers;
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
        /// <summary>
        /// 电源动作，保持原有的下拉顺序。
        ///
        /// 做成方法而不是 static 字段：Display 现在来自 <see cref="Strings"/> 查表，
        /// 需要在语言确定之后再构造，而不是在类型初始化时就把某一种语言的文本固化死。
        /// 每次调用返回新数组，调用方（包括 <c>*.Designer.cs</c>）请勿缓存结果。
        ///
        /// 注意：<see cref="LocalizedItem.Value"/> 是英文原文，是
        /// <see cref="Timer.ExecutePowerAction"/> 的 switch case、是 settings.json 的持久化值、
        /// 也是命令行 <c>/SetAction</c> 的参数，<b>任何时候都不能翻译</b>。
        /// </summary>
        public static LocalizedItem[] PowerActions()
        {
            return new LocalizedItem[]
            {
                new LocalizedItem("Shutdown",       Strings.T("Shutdown")),
                new LocalizedItem("Restart",        Strings.T("Restart")),
                new LocalizedItem("Hibernate",      Strings.T("Hibernate")),
                new LocalizedItem("Sleep",          Strings.T("Sleep")),
                new LocalizedItem("Logout",         Strings.T("Logout")),
                new LocalizedItem("Lock",           Strings.T("Lock")),
                new LocalizedItem("Custom Command", Strings.T("Custom Command"))
            };
        }

        /// <summary>
        /// 托盘图标主题，保持原有的下拉顺序。同样每次返回新数组，Value（Light / Dark）
        /// 被 <see cref="Countdown"/> 直接比较，不可翻译。
        /// </summary>
        public static LocalizedItem[] TrayIconThemes()
        {
            return new LocalizedItem[]
            {
                new LocalizedItem("Automatic", Strings.T("Automatic")),
                new LocalizedItem("Light",     Strings.T("Light")),
                new LocalizedItem("Dark",      Strings.T("Dark"))
            };
        }

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
