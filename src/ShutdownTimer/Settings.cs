using ShutdownTimer.Helpers;
using System;
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
            appLabel.Text = Application.ProductName + "@v" + Application.ProductVersion.Remove(Application.ProductVersion.LastIndexOf("."));
#if DEBUG
            appLabel.Text += "_debug";
#endif

            // 按当前语言统一应用字体，替换掉 Designer 里硬编码的微软雅黑。
            Localization.ApplyTo(this);

            LoadSettings();
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
                    DialogResult result = MessageBox.Show("禁用托盘图标后，程序在后台运行时将无法与其交互。\n遇到这种情况你只能用任务管理器强制结束程序。\n\n通知功能也会随之失效！\n\n确定要启用该选项吗？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
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
