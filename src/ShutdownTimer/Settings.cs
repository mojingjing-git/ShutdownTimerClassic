using ShutdownTimer.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ShutdownTimer
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            // 下拉框必须排在下面的 LoadSettings() 之前：LoadSettings() 内部调 SelectValue()，
            // 而 SelectValue() 首行就是 if (box.Items.Count == 0) return;
            // 填晚了就会静默 no-op → 下拉框 0 项 → GetValue() 返回 box.Text = "" →
            // settings.json 被写成 "Action": "" / "TrayIconTheme": ""，用户默认动作永久丢失。
            // 这两行以前是手工插在 InitializeComponent() 里的，在 VS 设计器里拖一次控件
            // 就会被重新生成无声删除，所以必须搬到这里。
            LocalizedCatalog.Fill(actionComboBox, LocalizedCatalog.PowerActions(), "Shutdown");
            LocalizedCatalog.Fill(trayiconThemeComboBox, LocalizedCatalog.TrayIconThemes(), "Automatic");
            Debug.Assert(actionComboBox.Items.Count > 0, "actionComboBox 必须在 LoadSettings() 之前填充");
            Debug.Assert(trayiconThemeComboBox.Items.Count > 0, "trayiconThemeComboBox 必须在 LoadSettings() 之前填充");

            ApplyTexts();

            appLabel.Text = Strings.T("Shutdown Timer Classic") + "@v" + Application.ProductVersion.Remove(Application.ProductVersion.LastIndexOf("."));
#if DEBUG
            appLabel.Text += "_debug";
#endif

            // 按当前语言统一应用字体，替换掉 Designer 里硬编码的微软雅黑。
            Localization.ApplyTo(this);

            // 必须排在 ApplyTo 之后：按钮宽度随字体变，量到的必须是最终渲染宽度。
            ReflowAboutButtons();

            LoadSettings();
        }

        /// <summary>
        /// 用 <see cref="Strings"/> 查表覆盖 <c>Settings.Designer.cs</c> 里硬编码的中文文案。
        ///
        /// 为什么不在 Designer 里直接改：<c>*.Designer.cs</c> 是 VS 自动生成的，
        /// 手工写进去的改动在下次打开设计器、拖动控件时会连同 InitializeComponent()
        /// 一起被重新生成覆盖掉。Designer 里的中文因此原样保留（开发者在设计器里
        /// 看到的仍是中文），运行时由本方法用同一份查表结果覆盖，界面行为零变化。
        ///
        /// key 一律是上游的英文原文，与 <c>upstream/master</c> 的 Designer 逐字一致。
        /// 译文照抄 Designer 里已有的中文，不改措辞、不改标点。
        /// </summary>
        private void ApplyTexts()
        {
            Text = Strings.T("Shutdown Timer - Settings");

            titleLabel.Text = Strings.T("Settings");
            footerLabel.Text = Strings.T("Made with love in Germany");
            githubLinkLabel.Text = Strings.T("View on GitHub");

            // 常规
            tabPage1.Text = Strings.T("General");
            groupBox1.Text = Strings.T("Application behaviour");
            enableMultipleInstances.Text = Strings.T("Allow multiple instances");
            rememberLastScreenPositionCountdown.Text = Strings.T("Remember last screen position (Countdown)");
            rememberLastScreenPositionUI.Text = Strings.T("Remember last screen position (Menu)");
            trayiconGroupBox.Text = Strings.T("System tray menu");
            trayiconThemeLabel.Text = Strings.T("Trayicon theme:");
            clearSettingsButton.Text = Strings.T("Clear Settings");
            defaultSettingsGroupBox.Text = Strings.T("Timer defaults");
            customDefaultsGroupBox.Text = Strings.T("Custom defaults");
            timeOfDayModeRadioButton.Text = Strings.T("At a specific time of day");
            countdownModeRadioButton.Text = Strings.T("After a specific timespan");
            secondsLabel.Text = Strings.T("Seconds");
            hoursLabel.Text = Strings.T("Hours");
            minutesLabel.Text = Strings.T("Minutes");
            preventSleepCheckBox.Text = Strings.T("Prevent system from going to sleep");
            gracefulCheckBox.Text = Strings.T("Graceful (do not force close apps)");
            backgroundCheckBox.Text = Strings.T("Run in background");
            actionLabel.Text = Strings.T("Select an action:");
            rememberStateCheckBox.Text = Strings.T("Remember last state");

            // 高级
            tabPage2.Text = Strings.T("Advanced");
            forceFlagGroupBox.Text = Strings.T("Force flag");
            forceFlagLabel.Text = Strings.T("Changes how running applications are force-closed when\r\nusing a non-graceful shutdown.");
            forceFlagDocsLinkLabel.Text = Strings.T("Documentation");
            countdownGroupBox.Text = Strings.T("Countdown window");
            disableAlwaysOnTopCheckBox.Text = Strings.T("Disable always on top behaviour");
            disableAnimationsCheckBox.Text = Strings.T("Disable animations");
            setBackgroundColorLinkLabel.Text = Strings.T("(set background color)");
            disableNotificationsCheckBox.Text = Strings.T("Disable notifications and confirmations");
            enableAdaptiveCountdownTextSizeCheckBox.Text = Strings.T("Adapt clock text to window size");
            transparentWindowCheckBox.Text = Strings.T("Transparent countdown window");
            hideTrayIconCheckBox.Text = Strings.T("Hide tray icon (not recommended)");
            passwordGroupBox.Text = Strings.T("Password protection");
            passwordLabel.Text = Strings.T("Locks the countdown UI with a password you can set when starting a shutdown after clicking start.");
            passwordCheckBox.Text = Strings.T("Enable password protection");
            developerGroupBox.Text = Strings.T("Developer Options");
            saveLogsCheckBox.Text = Strings.T("Save event logs to appdata on exit");
            openAppDataLinkLabel.Text = Strings.T("Open appdata folder");

            // 关于
            tabPage3.Text = Strings.T("About");
            aboutGroupBox.Text = Strings.T("About this app");
            logButton.Text = Strings.T("Create Logfile");
            emailbutton.Text = Strings.T("Email");
            githubButton.Text = Strings.T("GitHub Issues");
            licenseGroupBox.Text = Strings.T("Licenses && Source");
            appInfoLabel.Text = Strings.T("Application:");
            appLicenseLinkLabel.Text = Strings.T("MIT License");
            appSourceLinkLabel.Text = Strings.T("Sourcecode");
            faInfoLabel.Text = Strings.T("Font Awesome:");
            faLicenseLinkLabel.Text = Strings.T("CC BY 4.0 License");
            faSourceLinkLabel.Text = Strings.T("Sourcecode");
        }

        /// <summary>
        /// 「关于」页底部三个按钮按实际渲染宽度重新排布。
        ///
        /// 为什么需要：<c>Button.AutoSize</c> 默认是 true，本项目全项目没有一处显式设成 false
        /// （Designer 里的 <c>Size</c> 只是初始值）。汉化把这三个按钮的文本换成更长的中文后，
        /// 按钮会自动变宽，而 Designer 里 <c>emailbutton</c> 结束于 x=172、<c>githubButton</c>
        /// 起于 x=178 —— 只有 6px 间隙，于是 <c>githubButton</c> 自动扩展后的左边缘被
        /// <c>emailbutton</c> 的边框盖住，「GitHub 问题反馈」被截成「Hub 问题反馈」。
        ///
        /// 注意：这跟"文本超出控件被 Clip"是两回事。只用
        /// <c>TextRenderer.MeasureText(text, font) &gt; control.ClientSize.Width</c> 检测是
        /// 查不出来的——按钮自己已经变宽到能装下文本，问题是它压到了邻居身上。
        ///
        /// 所以这里按顺序重排 <c>Left</c>，保持统一的间隙，从第一个按钮的原位开始，
        /// 这样改动最小，也不会把整行推出 <c>aboutGroupBox</c>。
        /// <br />
        /// <b>调用时序：必须排在 <c>Localization.ApplyTo(this)</c> 之后。</b>按钮宽度取决于字体，
        /// 而这三个按钮都没有显式设 <c>Font</c>（走 <c>Form.Font</c> 传播，Designer 里也没设
        /// <c>this.Font</c>），所以在换字体之前量到的宽度并非最终宽度，排出的间隙也是按旧字体算的。
        /// </summary>
        private void ReflowAboutButtons()
        {
            const int gap = 8;
            Button[] row = { logButton, emailbutton, githubButton };

            int left = row[0].Left;
            foreach (Button button in row)
            {
                button.Left = left;
                left = button.Right + gap;
            }
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void RememberStateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (rememberStateCheckBox.Checked)
            {
                SettingsProvider.Settings.RememberLastState = true;
                customDefaultsGroupBox.Enabled = false;
            }
            else
            {
                SettingsProvider.Settings.RememberLastState = false;
                customDefaultsGroupBox.Enabled = true;
            }
        }

        private void ClearSettingsButton_Click(object sender, EventArgs e)
        {
            SettingsProvider.ClearSettings();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // general controls
            rememberStateCheckBox.Checked = SettingsProvider.Settings.RememberLastState;
            LocalizedCatalog.SelectValue(trayiconThemeComboBox, SettingsProvider.Settings.TrayIconTheme);
            rememberLastScreenPositionUI.Checked = SettingsProvider.Settings.RememberLastScreenPositionUI;
            rememberLastScreenPositionCountdown.Checked = SettingsProvider.Settings.RememberLastScreenPositionCountdown;
            enableMultipleInstances.Checked = SettingsProvider.Settings.EnableMultipleInstances;

            // default timer
            LocalizedCatalog.SelectValue(actionComboBox, SettingsProvider.Settings.DefaultTimer.Action);
            gracefulCheckBox.Checked = SettingsProvider.Settings.DefaultTimer.Graceful;
            preventSleepCheckBox.Checked = SettingsProvider.Settings.DefaultTimer.PreventSleep;
            backgroundCheckBox.Checked = SettingsProvider.Settings.DefaultTimer.Background;
            countdownModeRadioButton.Checked = SettingsProvider.Settings.DefaultTimer.CountdownMode;
            timeOfDayModeRadioButton.Checked = !SettingsProvider.Settings.DefaultTimer.CountdownMode;
            hoursNumericUpDown.Value = SettingsProvider.Settings.DefaultTimer.Hours;
            minutesNumericUpDown.Value = SettingsProvider.Settings.DefaultTimer.Minutes;
            secondsNumericUpDown.Value = SettingsProvider.Settings.DefaultTimer.Seconds;

            // advanced settings
            forceIfHungFlagRadioButton.Checked = SettingsProvider.Settings.ForceIfHungFlag;
            forceFlagRadioButton.Checked = !SettingsProvider.Settings.ForceIfHungFlag;
            disableAlwaysOnTopCheckBox.Checked = SettingsProvider.Settings.DisableAlwaysOnTop;
            disableAnimationsCheckBox.Checked = SettingsProvider.Settings.DisableAnimations;
            setBackgroundColorLinkLabel.Enabled = disableAnimationsCheckBox.Checked;
            disableNotificationsCheckBox.Checked = SettingsProvider.Settings.DisableNotifications;
            passwordCheckBox.Checked = SettingsProvider.Settings.PasswordProtection;
            enableAdaptiveCountdownTextSizeCheckBox.Checked = SettingsProvider.Settings.AdaptiveCountdownTextSize;
            hideTrayIconCheckBox.Checked = SettingsProvider.Settings.HideTrayIcon;
            if (SettingsProvider.Settings.BackgroundColor == Color.Transparent) { transparentWindowCheckBox.Checked = true; }
            saveLogsCheckBox.Checked = SettingsProvider.Settings.SaveEventLogOnExit;
        }

        private void SaveSettings()
        {
            // general controls
            SettingsProvider.Settings.RememberLastState = rememberStateCheckBox.Checked;
            SettingsProvider.Settings.TrayIconTheme = LocalizedCatalog.GetValue(trayiconThemeComboBox);
            SettingsProvider.Settings.RememberLastScreenPositionUI = rememberLastScreenPositionUI.Checked;
            SettingsProvider.Settings.RememberLastScreenPositionCountdown = rememberLastScreenPositionCountdown.Checked;
            SettingsProvider.Settings.EnableMultipleInstances = enableMultipleInstances.Checked;

            // default timer
            if (!SettingsProvider.Settings.RememberLastState)
            {
                SettingsProvider.Settings.DefaultTimer.Action = LocalizedCatalog.GetValue(actionComboBox);
                SettingsProvider.Settings.DefaultTimer.Graceful = gracefulCheckBox.Checked;
                SettingsProvider.Settings.DefaultTimer.PreventSleep = preventSleepCheckBox.Checked;
                SettingsProvider.Settings.DefaultTimer.Background = backgroundCheckBox.Checked;
                SettingsProvider.Settings.DefaultTimer.CountdownMode = countdownModeRadioButton.Checked;
                SettingsProvider.Settings.DefaultTimer.Hours = Convert.ToInt32(hoursNumericUpDown.Value);
                SettingsProvider.Settings.DefaultTimer.Minutes = Convert.ToInt32(minutesNumericUpDown.Value);
                SettingsProvider.Settings.DefaultTimer.Seconds = Convert.ToInt32(secondsNumericUpDown.Value);
            }

            // advanced settings
            SettingsProvider.Settings.ForceIfHungFlag = forceIfHungFlagRadioButton.Checked;
            SettingsProvider.Settings.DisableAlwaysOnTop = disableAlwaysOnTopCheckBox.Checked;
            SettingsProvider.Settings.DisableAnimations = disableAnimationsCheckBox.Checked;
            SettingsProvider.Settings.DisableNotifications = disableNotificationsCheckBox.Checked;
            SettingsProvider.Settings.AdaptiveCountdownTextSize = enableAdaptiveCountdownTextSizeCheckBox.Checked;
            SettingsProvider.Settings.HideTrayIcon = hideTrayIconCheckBox.Checked;
            SettingsProvider.Settings.PasswordProtection = passwordCheckBox.Checked;
            SettingsProvider.Settings.SaveEventLogOnExit = saveLogsCheckBox.Checked;

            SettingsProvider.Save();
        }

        private void GithubLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/mojingjing-git/ShutdownTimerClassic");
        }

        private void AppLicenseLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/mojingjing-git/ShutdownTimerClassic/blob/master/LICENSE");
        }

        private void AppSourceLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/mojingjing-git/ShutdownTimerClassic");
        }

        private void FALicenseLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://fontawesome.com/license/free");
        }

        private void FASourceLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/FortAwesome/Font-Awesome");
        }

        private void GithubButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/mojingjing-git/ShutdownTimerClassic/issues");
        }

        private void EmailButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:lukas.langrock@outlook.de");
        }

        private void ForceFlagDocsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-exitwindowsex#parameters");
        }

        private void LogButton_Click(object sender, EventArgs e)
        {
            ExceptionHandler.CreateManualLog();
        }

        private void SetBackgroundColorLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog
            {
                Color = SettingsProvider.Settings.BackgroundColor,
                FullOpen = true
            };
            DialogResult result = colorDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                SettingsProvider.Settings.BackgroundColor = colorDialog.Color;
            }
        }

        private void TransparentWindowCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (transparentWindowCheckBox.Checked)
            {
                SettingsProvider.Settings.BackgroundColor = Color.Transparent;
            }
            else
            {
                SettingsProvider.Settings.BackgroundColor = Color.Blue;
            }
        }

        private void DisableAnimationsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            setBackgroundColorLinkLabel.Enabled = disableAnimationsCheckBox.Checked;
            transparentWindowCheckBox.Enabled = disableAnimationsCheckBox.Checked;
        }

        private void openAppDataLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Shutdown Timer Classic");
        }

        private void hideTrayIconCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (hideTrayIconCheckBox.Checked)
            {
                // hiding the trayicon prevents notifications
                disableNotificationsCheckBox.Enabled = false;

                // ensure user understands implications from this action
                if (hideTrayIconCheckBox.Focused)
                {
                    DialogResult result = MessageBox.Show(Strings.T("Disabling the tray icon will prevent you from interacting with the application when it's running in the background.\nYour only choice in such a case is to kill the application with Task Manager.\n\nNotifications will also stop working!\n\nAre you sure you want to enable this option?"), Strings.T("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes)
                    {
                        hideTrayIconCheckBox.Checked = false;
                    }
                }
            } else
            {
                // hiding the trayicon prevents notifications
                disableNotificationsCheckBox.Enabled = true;
            }
        }
    }
}
