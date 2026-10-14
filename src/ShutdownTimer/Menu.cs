using ShutdownTimer.Helpers;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ShutdownTimer
{
    public partial class Menu : Form
    {
        public bool ApplyArgumentValues { get; set; }
        public int ArgTimeH { get; set; }
        public int ArgTimeM { get; set; }
        public int ArgTimeS { get; set; }
        public string ArgAction { get; set; }
        public string ArgMode { get; set; }
        public string ArgPassword { get; set; }
        public string ArgCommand { get; set; }
        public bool ArgGraceful { get; set; }
        public bool ArgPreventSleep { get; set; }
        public bool ArgBackground { get; set; }
        public bool ArgUseTimeOfDay { get; set; }

        private string password; // used for password protection
        private string command; // used for custom command
        private bool isConcluded = false; // true after form has concluded its function

        public Menu()
        {
            InitializeComponent();
        }

        #region "form events"

        private void Menu_Load(object sender, EventArgs e)
        {
            ExceptionHandler.Log("Check multiple instances setting");

            if (!SettingsProvider.Settings.EnableMultipleInstances)
            {
                ExceptionHandler.Log("Multiple instances disabled");

                ExceptionHandler.Log("Checking for running instance");
                if (!ApplicationInstanceManager.IsSingleInstance())
                {
                    MessageBox.Show("本程序已在运行。若要允许多开，请在应用设置中勾选“允许多开”选项。\n\n即将退出……", "程序已在运行！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ExceptionHandler.Log("Another instance detected; exiting");
                    Application.Exit();
                }
            }
            else
            {
                ExceptionHandler.Log("Multiple instances allowed");
            }

            ExceptionHandler.Log("Initializing form");

            versionLabel.Text = "v" + Application.ProductVersion.Remove(Application.ProductVersion.LastIndexOf(".")); // Display current version
#if DEBUG
            versionLabel.Text += "_debug";
#endif

            infoToolTip.SetToolTip(gracefulCheckBox, "默认情况下，未响应关闭提示的程序会被强制结束，以确保关机顺利完成。\n\n而“安全关机”会等待所有程序自行退出后再继续关机。\n如果某个程序无响应，或需要用户交互才能退出，可能会导致关机失败。");
            infoToolTip.SetToolTip(preventSleepCheckBox, "根据系统的电源设置，一段时间无操作后电脑可能会自动进入睡眠。\n启用此选项可让系统保持唤醒，确保计时器能正常运行并准时执行动作。");
            infoToolTip.SetToolTip(backgroundCheckBox, "启动倒计时后不显示窗口，仅在任务栏托盘显示一个图标。");
            infoToolTip.SetToolTip(countdownModeRadioButton, "从下方设置的时、分、秒开始倒数，\n就像普通计时器一样，归零后执行所选动作。");
            infoToolTip.SetToolTip(timeOfDayModeRadioButton, "在此模式下可选择目标时刻（24 小时制）。\n若该时刻已过，将自动顺延至次日。\n\n点击开始后，程序会据此计算倒计时时长。\n");

            // 使用微软雅黑以正确显示中文字符，避免字体回退导致的布局问题。
            this.Font = new Font("Microsoft YaHei UI", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);

            ExceptionHandler.Log("Form initialized");
        }

        private void Menu_Shown(object sender, EventArgs e)
        {
            ExceptionHandler.Log("Form shown");
            // Check for startup arguments
            if (ApplyArgumentValues)
            {
                // Apply given setting
                ExceptionHandler.Log("Applying CLI arguments");
                LoadArgs();
            }
            else
            {
                // Load settings
                ExceptionHandler.Log("Loading settings");
                Application.DoEvents();
                LoadSettings();
            }
        }

        private void Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExceptionHandler.Log("Form closing");

            // Only save settings when form hasn't concluded it's function.
            if (!isConcluded)
            {
                SaveSettings();
            }
        }

        private void ActionComboBox_TextChanged(object sender, EventArgs e)
        {
            // disables graceful checkbox for all modes which can not be executed gracefully / which always execute gracefully
            string selectedAction = LocalizedCatalog.GetValue(actionComboBox);
            if (selectedAction == "Shutdown" || selectedAction == "Restart" || selectedAction == "Logout") { gracefulCheckBox.Enabled = true; }
            else { gracefulCheckBox.Enabled = false; }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            ExceptionHandler.Log("Showing settings form");
            Settings settings = new Settings();
            settings.ShowDialog();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            ExceptionHandler.Log("Start requested");

            (bool allChecksPassed, string listOfErrorsFound, string listOfWarningsFound) = RunChecks();

            if (!allChecksPassed)
            {
                ExceptionHandler.Log("Start aborted: failing checks");
                MessageBox.Show("发生以下错误：\n\n" + listOfErrorsFound + "请解决上述问题后重试。", "似乎出现了问题！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!listOfWarningsFound.Equals(""))
            {
                if (MessageBox.Show(listOfWarningsFound + "是否仍要继续？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                {
                    ExceptionHandler.Log("User didn't accept the warnings; aborting");
                    return;
                }
            }

            if (!String.IsNullOrEmpty(ArgCommand))
            {
                command = ArgCommand;
            }
            else if (LocalizedCatalog.GetValue(actionComboBox) == "Custom Command")
            {
                ExceptionHandler.Log("Custom command requested");
                using (var form = new InputBox())
                {
                    form.Title = "自定义命令";

                    ExceptionHandler.Log("Prompting for custom command");
                    var result = form.ShowDialog();

                    if (result != DialogResult.OK)
                    {
                        ExceptionHandler.Log("User cancelled custom command input; aborting");
                        return;
                    }

                    if (String.IsNullOrWhiteSpace(form.ReturnValue))
                    {
                        ExceptionHandler.Log("Invalid custom command input; aborting");
                        MessageBox.Show("自定义命令不能为空。请输入有效命令，或改用其它动作！", "无效命令！", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ExceptionHandler.Log("Accepted command: \"" + form.ReturnValue + "\".");
                    command = form.ReturnValue;
                }
            }

            if (!String.IsNullOrEmpty(ArgPassword))
            {
                password = ArgPassword;
            }
            else if (SettingsProvider.Settings.PasswordProtection)
            {
                ExceptionHandler.Log("Password protection enabled");
                using (var form = new InputBox())
                {
                    form.Title = "密码保护";
                    form.PasswordMode = true;
                    ExceptionHandler.Log("Prompting for password");
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        ExceptionHandler.Log("Password set");
                        password = form.ReturnValue;
                    } else
                    {
                        ExceptionHandler.Log("User declines password prompt; aborting");
                        return;
                    }
                }
            }

            StartCountdown();
        }

        #endregion

        /// <summary>
        /// Checks user input before further processing
        /// </summary>
        /// <returns>Report of all failed checks</returns>
        private (bool allChecksPassed, string listOfErrorsFound, string ListOfWarningsFound) RunChecks()
        {
            ExceptionHandler.Log("Running checks...");

            string errMessages = ""; // error messages will append to this
            string warnMessages = ""; // warning messages will append to this

            // Check if chosen action is a valid option
            if (actionComboBox.SelectedItem == null)
            {
                errMessages += "请从下拉菜单中选择一个有效动作！\n\n";
            }

            // Check if all time values are zero when in countdown mode
            if (hoursNumericUpDown.Value == 0 && minutesNumericUpDown.Value == 0 && secondsNumericUpDown.Value == 0 && countdownModeRadioButton.Checked)
            {
                errMessages += "倒计时模式下，计时不能从 0 开始！\n\n";
            }

            // Respective check for either countdown or timeOfDay mode
            try
            {
                DateTimeOffset nowReference = DateTimeOffset.Now;
                TimeSpan ts = Time.CalculateCountdownTimeSpan(hoursNumericUpDown.Value, minutesNumericUpDown.Value, secondsNumericUpDown.Value, timeOfDayModeRadioButton.Checked, nowReference);

                // Sanity check
                if (ts.TotalDays > 100)
                {
                    warnMessages += $"你设置的时间相当于 {Math.Round(ts.TotalDays)} 天（约 {Math.Round(ts.TotalDays / 365, 2)} 年）！\n" +
                        "这时间有点长，本程序从未针对如此长的运行时设计，祝你好运！\n\n";
                }

                // Check for time discontinuities when using timeOfDay mode
                if (timeOfDayModeRadioButton.Checked)
                {
                    
                    DateTimeOffset actualEnd = Time.CalculateLocalEndTime(ts, nowReference, TimeZoneInfo.Local);
                    TimeSpan diff = actualEnd.Offset - nowReference.Offset;
                    ExceptionHandler.Log($"Time discontinuity check: {diff.TotalSeconds} seconds diff detected");

                    if (diff.TotalSeconds != 0)
                    {
                        warnMessages += $"倒计时期间你的本地时钟会发生跳变！实际结束时刻与你设定的目标相差 {Math.Abs(diff.TotalSeconds)} 秒，约合 {Math.Abs(diff.TotalHours)} 小时。这通常由夏令时切换或本地时区的其它时间调整事件引起。\n\n" +
                            $"倒计时按当前时刻与你设定的 {nowReference.Add(ts):HH:mm:ss} 之差计算。" +
                            $"但计时结束时，你的时钟会显示 {actualEnd:HH:mm:ss}。\n\n" +
                            $"倒计时始终采用简单算术，而不会考虑时区规则，请留意这一差异！\n\n";
                    }
                }
            }
            catch
            {
                errMessages += "时间跨度换算失败！请检查各时间值是否在合理范围内；如果你使用了定时模式，请确认所填时刻是否有效。\n\n";
            }

            // Check if we can get permissions forExitWindowsEx() (used for shutdown/reboot/logoff)
            try
            {
                string permissionAction = LocalizedCatalog.GetValue(actionComboBox);
                if (permissionAction == "Shutdown" || permissionAction == "Restart" || permissionAction == "Logout")
                {
                    ExitWindows.GetPrivileges(); // throws on error
                }
            }
            catch (Win32Exception ex)
            {
                errMessages += "权限不足，无法向 Windows 申请关机/重启/注销所需的权限。错误信息：" + ex.Message + "\n\n";
                errMessages += "如需了解此问题的详细信息，请进入应用设置页面，在“关于”选项卡中点击“创建日志文件”。";
            }

            if (errMessages.Equals(""))
            {
                ExceptionHandler.Log("Ran all checks, no errors found.");
                return (true, errMessages, warnMessages);
            }
            else
            {
                ExceptionHandler.Log("Ran all checks, the following errors have been found:\n" + errMessages);
                ExceptionHandler.Log("as well as the following warnings:\n" + warnMessages);
                return (false, errMessages, warnMessages);
            }
        }

        /// <summary>
        /// Load UI element data from args
        /// </summary>
        private void LoadArgs()
        {
            LocalizedCatalog.SelectValue(actionComboBox, ArgAction);
            gracefulCheckBox.Checked = ArgGraceful;
            preventSleepCheckBox.Checked = ArgPreventSleep;
            backgroundCheckBox.Checked = ArgBackground;
            hoursNumericUpDown.Value = ArgTimeH;
            minutesNumericUpDown.Value = ArgTimeM;
            secondsNumericUpDown.Value = ArgTimeS;
            countdownModeRadioButton.Checked = !ArgUseTimeOfDay;
            timeOfDayModeRadioButton.Checked = ArgUseTimeOfDay;

            if (ArgMode.Equals("Lock"))
            {
                ExceptionHandler.Log("Setting 'Lock' mode");
                startButton.Text = "开始（使用推荐设置）";
                settingsButton.Enabled = false;
                actionGroupBox.Enabled = false;
                timeGroupBox.Enabled = false;
            }
        }

        /// <summary>
        /// Load UI element data from settings
        /// </summary>
        private void LoadSettings()
        {
            LocalizedCatalog.SelectValue(actionComboBox, SettingsProvider.Settings.DefaultTimer.Action);
            gracefulCheckBox.Checked = SettingsProvider.Settings.DefaultTimer.Graceful;
            preventSleepCheckBox.Checked = SettingsProvider.Settings.DefaultTimer.PreventSleep;
            backgroundCheckBox.Checked = SettingsProvider.Settings.DefaultTimer.Background;
            countdownModeRadioButton.Checked = SettingsProvider.Settings.DefaultTimer.CountdownMode;
            timeOfDayModeRadioButton.Checked = !SettingsProvider.Settings.DefaultTimer.CountdownMode;
            hoursNumericUpDown.Value = SettingsProvider.Settings.DefaultTimer.Hours;
            minutesNumericUpDown.Value = SettingsProvider.Settings.DefaultTimer.Minutes;
            secondsNumericUpDown.Value = SettingsProvider.Settings.DefaultTimer.Seconds;

            // Check if the user has opted to remember the last UI screen position,
            // and if LastScreenPosition is available (not null), apply its X and Y coordinates to position the form.
            if (SettingsProvider.Settings.RememberLastScreenPositionUI && SettingsProvider.Settings.LastScreenPositionUI != null)
            {
                this.Location = new Point(SettingsProvider.Settings.LastScreenPositionUI.X, SettingsProvider.Settings.LastScreenPositionUI.Y);
            }
        }

        /// <summary>
        /// Saves current timer settings as default settings if activated in settings
        /// and the last UI position
        /// </summary>
        private void SaveSettings()
        {
            if (SettingsProvider.SettingsLoaded)
            {
                ExceptionHandler.Log("Saving settings...");

                if (SettingsProvider.Settings.RememberLastState)
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

                if (SettingsProvider.Settings.RememberLastScreenPositionUI)
                {
                    SettingsProvider.Settings.LastScreenPositionUI = new LastScreenPosition
                    {
                        X = this.Location.X,
                        Y = this.Location.Y
                    };
                }

                SettingsProvider.Save();
                ExceptionHandler.Log("Settings saved");
            }
            else
            {
                ExceptionHandler.Log("Ignoring SaveSettings() call as no settings are loaded");
            }
        }

        /// <summary>
        /// Starts the countdown with values from UI
        /// </summary>
        private void StartCountdown()
        {
            ExceptionHandler.Log("Preparing countdown");
            startButton.Enabled = false;
            actionGroupBox.Enabled = false;
            timeGroupBox.Enabled = false;
            isConcluded = true; // mark form as concluded to prevent further logic on form closing events
            SaveSettings();

            ExceptionHandler.Log("Countdown initiated");
            this.Hide();

            ExceptionHandler.Log("Starting countdown...");

            // Calculate TimeSpan
            ExceptionHandler.Log("Calculating timespan");
            TimeSpan timeSpan = Time.CalculateCountdownTimeSpan(hoursNumericUpDown.Value, minutesNumericUpDown.Value, secondsNumericUpDown.Value, timeOfDayModeRadioButton.Checked, DateTime.Now);

            Timer.CountdownTimeSpan = timeSpan;
            Timer.Action = LocalizedCatalog.GetValue(actionComboBox);
            Timer.Graceful = gracefulCheckBox.Checked;
            Timer.PreventSystemSleep = preventSleepCheckBox.Checked;
            Timer.Command = command;

            // Show countdown window
            ExceptionHandler.Log("Starting timer");
            Timer.Start(password, !backgroundCheckBox.Checked);
        }
    }
}
