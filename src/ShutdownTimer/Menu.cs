using ShutdownTimer.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
            // 下拉框必须在 LoadArgs() / LoadSettings()（由 Menu_Shown 触发，晚于 Load）之前填好，
            // 否则 SelectValue() 会因为 Items.Count == 0 而静默 no-op。
            // 这行以前是手工插在 InitializeComponent() 里的：在 VS 里打开设计器拖动控件，
            // 重新生成 InitializeComponent() 就会把它无声删掉，所以必须放在这里。
            LocalizedCatalog.Fill(actionComboBox, LocalizedCatalog.PowerActions(), "Shutdown");

            ApplyTexts();

            ExceptionHandler.Log("Check multiple instances setting");

            if (!SettingsProvider.Settings.EnableMultipleInstances)
            {
                ExceptionHandler.Log("Multiple instances disabled");

                ExceptionHandler.Log("Checking for running instance");
                if (!ApplicationInstanceManager.IsSingleInstance())
                {
                    MessageBox.Show(Strings.T("Another instance of this application is already running. To allow multiple instances, please check the \"Allow multiple instances\" option in the application settings.\n\nExiting... (Main menu)"), Strings.T("Application already running!"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            infoToolTip.SetToolTip(gracefulCheckBox, Strings.T("Applications that do not exit when prompted automatically get terminated by default to ensure a successful shutdown.\n\nA graceful shutdown, on the other hand, will wait for all applications to exit before continuing with the shutdown.\nThis might result in an unsuccessful shutdown if one or more applications are unresponsive or require a user interaction to exit."));
            infoToolTip.SetToolTip(preventSleepCheckBox, Strings.T("Depending on the power settings of your system, it might go to sleep after a certain amount of time due to inactivity.\nThis option will keep the system awake to ensure the timer can properly run and execute a shutdown."));
            infoToolTip.SetToolTip(backgroundCheckBox, Strings.T("This will launch the countdown without a visible window but will show a tray icon in your taskbar."));
            infoToolTip.SetToolTip(countdownModeRadioButton, Strings.T("Will count down from the hours, minutes and seconds selected below,\nlike a countdown timer, and execute the power action when it reaches zero."));
            infoToolTip.SetToolTip(timeOfDayModeRadioButton, Strings.T("In this mode you can select the target time of day (24h clock) for the power action.\nIf the time has already passed, it will roll over to tomorrow.\n\nWhen you press start, the appropriate countdown will be calculated.\n"));

            // 按当前语言统一应用字体，替换掉 Designer 里硬编码的微软雅黑。
            Localization.ApplyTo(this);

            ExceptionHandler.Log("Form initialized");
        }

        /// <summary>
        /// 用 <see cref="Strings"/> 查表覆盖 <c>Menu.Designer.cs</c> 里硬编码的中文文案。
        ///
        /// 为什么不在 Designer 里直接改：<c>*.Designer.cs</c> 是 VS 自动生成的，
        /// 手工写进去的改动在下次打开设计器、拖动控件时会连同 InitializeComponent()
        /// 一起被重新生成覆盖掉。Designer 里的中文因此原样保留（开发者在设计器里
        /// 看到的仍是中文），运行时由本方法用同一份查表结果覆盖，界面行为零变化。
        ///
        /// key 一律是上游的英文原文，与 <c>upstream/master</c> 的 Designer 逐字一致。
        /// </summary>
        private void ApplyTexts()
        {
            Text = Strings.T("Shutdown Timer");
            AccessibleName = Strings.T("Shutdown Timer");
            AccessibleDescription = Strings.T("The main window where you can choose the time and power action.");

            titleLabel.Text = Strings.T("Shutdown Timer");

            hoursLabel.Text = Strings.T("Hours");
            minutesLabel.Text = Strings.T("Minutes");
            secondsLabel.Text = Strings.T("Seconds");

            timeGroupBox.Text = Strings.T("When to do it?");
            timeOfDayModeRadioButton.Text = Strings.T("At a specific time of day");
            countdownModeRadioButton.Text = Strings.T("After a specific timespan");

            actionGroupBox.Text = Strings.T("What to do?");
            actionLabel.Text = Strings.T("Select an action:");
            preventSleepCheckBox.Text = Strings.T("Prevent system from going to sleep");
            gracefulCheckBox.Text = Strings.T("Graceful (do not force close apps)");
            backgroundCheckBox.Text = Strings.T("Run in background");

            startButton.Text = Strings.T("Start");
            infoToolTip.ToolTipTitle = Strings.T("Help");
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

            CheckResult checkResult = RunChecks();

            if (!checkResult.AllChecksPassed)
            {
                ExceptionHandler.Log("Start aborted: failing checks");
                // 结尾的 "\n\n" 同上：上游是靠最后一条错误自带的换行把页脚顶到下一段，
                // 拆成独立 Issue 后那条换行没了，不补回来的话最后一条错误会和页脚连成一行。
                MessageBox.Show(
                    Strings.T("The following error(s) occurred:\n\n") + Render(checkResult, IssueSeverity.Error) + "\n\n" + Strings.T("Please resolve the problem(s) and try again."),
                    Strings.T("There seems to be a problem!"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (checkResult.Issues.Any(i => i.Severity == IssueSeverity.Warning))
            {
                // 注意结尾的 "\n\n"：上游英文是靠最后一条警告自带的 "\n\n" 把这句提示顶到下一段，
                // 拆成独立 Issue 后那条换行没了，必须在这里补回来，否则会连成一行。
                if (MessageBox.Show(Render(checkResult, IssueSeverity.Warning) + "\n\n" + Strings.T("Do you wish to continue?"), Strings.T("Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
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
                    form.Title = Strings.T("Custom Command");

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
                        MessageBox.Show(Strings.T("Custom command field was empty. Please enter a valid command or use a different action!"), Strings.T("Invalid command!"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    form.Title = Strings.T("Password Protection");
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
        /// <see cref="RunChecks"/> 的产物：只装结构化结果，文案在渲染时才查表。
        /// </summary>
        private sealed class CheckResult
        {
            /// <summary>只有一条 <see cref="IssueSeverity.Error"/> 就会阻断启动，且无法被用户确认绕过。</summary>
            public bool AllChecksPassed => !Issues.Any(i => i.Severity == IssueSeverity.Error);

            public List<Issue> Issues { get; } = new List<Issue>();
        }

        /// <summary>
        /// TimeSpan 换算失败（B5）用的英文原文键。
        /// </summary>
        /// <remarks>
        /// <b>安全约束：这个 key 对应的 Issue 必须是 <see cref="IssueSeverity.Error"/>，永远不能改成
        /// <see cref="IssueSeverity.Warning"/>。</b>
        /// <br />
        /// 原因：B5 触发时倒计时的目标时刻根本没算出来。若它只是警告，用户会看到一个「是否仍要继续？」
        /// 警告框，点「是」就带着一个算不出来的时间跨度直接进入倒计时并执行关机动作——
        /// 从「明确报错」退化成「静默错误」，可能直接关机。这是本次改造里唯一能出人命的约束。
        /// <br />
        /// <b>防线（按强度排序）：</b>① 该 Issue 必须经 <see cref="Issue.Blocking"/> 产生——
        /// 它把 Severity 硬编码成 Error，而 <see cref="Issue.Severity"/> 是只读属性，
        /// 「把它改成 Warning」在语言层面就写不出来，且与构建配置无关，Release 下同样生效；
        /// ② <see cref="RunChecks"/> 末尾还有一条 <see cref="Debug.Assert"/> 二次校验，但注意它走
        /// <c>[Conditional("DEBUG")]</c>，Release 编译时被整段剔除，<b>只能当辅助，不能当唯一防线</b>。
        /// </remarks>
        private const string KeyTimeSpanConversionFailed = "TimeSpan conversion failed! Please check if your time values are within a reasonable range or represent a valid time of day, if you used the time of day mode.";

        /// <summary>
        /// 把指定级别的检查结果渲染成一段界面文本：逐条查表，段与段之间用一个空行分隔。
        /// </summary>
        private static string Render(CheckResult result, IssueSeverity severity)
        {
            return string.Join("\n\n", result.Issues.Where(i => i.Severity == severity).Select(i => Strings.T(i.Key, i.Args)));
        }

        /// <summary>
        /// Checks user input before further processing
        /// </summary>
        /// <returns>全部检查结果：错误与警告以 <see cref="IssueSeverity"/> 区分，错误优先阻断启动</returns>
        private CheckResult RunChecks()
        {
            ExceptionHandler.Log("Running checks...");

            CheckResult result = new CheckResult();

            // Check if chosen action is a valid option
            if (actionComboBox.SelectedItem == null)
            {
                result.Issues.Add(Issue.Blocking("Please select a valid action from the dropdown menu!"));
            }

            // Check if all time values are zero when in countdown mode
            if (hoursNumericUpDown.Value == 0 && minutesNumericUpDown.Value == 0 && secondsNumericUpDown.Value == 0 && countdownModeRadioButton.Checked)
            {
                result.Issues.Add(Issue.Blocking("The timer cannot start at 0 when in countdown mode!"));
            }

            // Respective check for either countdown or timeOfDay mode
            try
            {
                DateTimeOffset nowReference = DateTimeOffset.Now;
                TimeSpan ts = Time.CalculateCountdownTimeSpan(hoursNumericUpDown.Value, minutesNumericUpDown.Value, secondsNumericUpDown.Value, timeOfDayModeRadioButton.Checked, nowReference);

                // Sanity check
                if (ts.TotalDays > 100)
                {
                    // 传原始 double（保留几位小数由译文决定），不要传 Math.Round 之后插值好的字符串。
                    result.Issues.Add(Issue.Advisory(
                        "Your chosen time equates to {0} days ({1} years)!\nThis is a bit much, and the application was never designed for this kind of runtime, so good luck!",
                        Math.Round(ts.TotalDays), Math.Round(ts.TotalDays / 365, 2)));
                }

                // Check for time discontinuities when using timeOfDay mode
                if (timeOfDayModeRadioButton.Checked)
                {
                    
                    DateTimeOffset actualEnd = Time.CalculateLocalEndTime(ts, nowReference, TimeZoneInfo.Local);
                    TimeSpan diff = actualEnd.Offset - nowReference.Offset;
                    ExceptionHandler.Log($"Time discontinuity check: {diff.TotalSeconds} seconds diff detected");

                    if (diff.TotalSeconds != 0)
                    {
                        // 原本是一整段中文，拆成 4 条：每条一个语义单元，译者才能调整语序。
                        // Args 一律传原始值：秒/小时传 double，时刻传 DateTimeOffset，
                        // 格式说明（{0:HH:mm:ss}）写进 key，绝不在这里预先插值。
                        result.Issues.Add(Issue.Advisory(
                            "Your local clock will shift during this countdown! The actual time the countdown will end on diverges from your target by {0} seconds, that's {1} hour(s). This can be caused by daylight savings transition or other scheduled events in your local timezone.",
                            Math.Abs(diff.TotalSeconds), Math.Abs(diff.TotalHours)));
                        result.Issues.Add(Issue.Advisory(
                            "The countdown will use the difference between now and your target of {0:HH:mm:ss}.",
                            nowReference.Add(ts)));
                        result.Issues.Add(Issue.Advisory(
                            "However, your clock will display {0:HH:mm:ss} when the timer ends.",
                            actualEnd));
                        result.Issues.Add(Issue.Advisory(
                            "The countdown will always use simple arithmetic instead of relying on timezone-aware calculation. Please be aware of this discrepancy!"));
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Log("TimeSpan conversion check failed: " + ex.GetType().Name + ": " + ex.Message);
                // 安全约束：见 KeyTimeSpanConversionFailed 的注释——必须是 Error。
                // 必须走 Blocking 工厂：Severity 构造后不可变，这条防线在 Release 下同样生效。
                result.Issues.Add(Issue.Blocking(KeyTimeSpanConversionFailed));
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
                // 拆成两条独立 Issue：上游靠第一条自带的 "\n\n" 把第二条顶下去，
                // 那条换行在旧的中文硬编码里已经丢了，会让两句话连成一行。
                result.Issues.Add(Issue.Blocking(
                    "Permission failure. Couldn't enable the required privliges for Shutdown/Restart/Logout from Windows. Error: {0}",
                    ex.Message));
                result.Issues.Add(Issue.Blocking(
                    "To learn more about this issue, please go the application's settings and in the \"About\"-tab, click on \"Create Logfile\"."));
            }

            // 开发期二次校验。注意 Debug.Assert 走 [Conditional("DEBUG")]，Release 下整段消失，
            // 它挡不住任何东西——B5 真正靠得住的防线是 Issue.Blocking + Issue.Severity 只读属性。
            Debug.Assert(
                result.Issues.Where(i => i.Key == KeyTimeSpanConversionFailed).All(i => i.Severity == IssueSeverity.Error),
                "B5 (TimeSpan conversion failed) must stay IssueSeverity.Error: the target time could not be computed, and a Warning would let the user confirm and shut the machine down anyway.");

            if (result.AllChecksPassed)
            {
                ExceptionHandler.Log("Ran all checks, no errors found.");
            }
            else
            {
                ExceptionHandler.Log("Ran all checks, the following errors have been found:\n" + Render(result, IssueSeverity.Error));
                ExceptionHandler.Log("as well as the following warnings:\n" + Render(result, IssueSeverity.Warning));
            }

            return result;
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
                startButton.Text = Strings.T("Start (with recommended settings)");
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

    /// <summary>
    /// 一条检查结果的严重级别。
    /// </summary>
    /// <remarks>
    /// 关键安全约定：只要存在 <see cref="Error"/>，<see cref="Menu"/> 的检查结果就会阻断启动，
    /// 用户无法点「仍然继续」绕过。<see cref="Warning"/> 则允许用户确认后继续，
    /// 因此任何「结果不可信、算出来的值不能用」的情况都必须标成
    /// <see cref="Error"/>，不能标成 <see cref="Warning"/>。
    /// </remarks>
    internal enum IssueSeverity { Error, Warning }

    /// <summary>
    /// 一条检查结果：只描述「发生了什么」，不持有任何已渲染的界面文本。
    /// </summary>
    /// <remarks>
    /// 注意：本类型（连同 <see cref="IssueSeverity"/>）必须留在 Menu.cs 的 <b>Menu 类之后</b>。
    /// MSBuild 的 CreateCSharpManifestResourceName 用「同名 .cs 文件里第一个声明的类型」决定
    /// Menu.resx 的清单资源名；一旦把 Issue 提到 Menu 前面，资源名会变成
    /// ShutdownTimer.Issue.resources，Designer 里的
    /// <c>new ResourceManager("ShutdownTimer.Menu", …)</c> 就找不到图标，程序一启动就崩溃。
    /// </remarks>
    internal sealed class Issue
    {
        /// <summary>英文原文键，交由 <see cref="Strings.T(string, object[])"/> 查表。</summary>
        public string Key { get; private set; }

        /// <summary>
        /// 格式化参数，<b>必须传原始值</b>（DateTimeOffset / double 等），不要传插值好的字符串：
        /// 格式说明（如 <c>{0:HH:mm:ss}</c>）写在 <see cref="Key"/> 里，由译文决定怎么排版。
        /// </summary>
        public object[] Args { get; private set; }

        /// <summary>
        /// 严重级。<b>构造后不可变</b>——只能经 <see cref="Blocking"/> 或 <see cref="Advisory"/> 产生。
        /// </summary>
        /// <remarks>
        /// 这里刻意不给 setter：属性可写时，「把某条阻断项误改成 Warning」只是一次看似无害的编辑，
        /// 而对 B5 来说它等于让用户点「是」就带着算不出来的时间跨度直接关机。
        /// 做成只读 + 工厂后，这种误改在语言层面就无从写起，且与构建配置无关——
        /// <c>Debug.Assert</c> 受 <c>[Conditional("DEBUG")]</c> 影响，Release 下会被整段剔除，
        /// 拿它当唯一防线是假的。
        /// </remarks>
        public IssueSeverity Severity { get; private set; }

        private Issue(string key, IssueSeverity severity, object[] args)
        {
            Key = key;
            Severity = severity;
            Args = args;
        }

        /// <summary>
        /// 阻断级：只要存在一条，<see cref="CheckResult.AllChecksPassed"/> 即为 false，
        /// 用户无法点「仍然继续」绕过。
        /// </summary>
        /// <remarks>
        /// 下面这个重载不是冗余：<c>params</c> 在只传一个字符串实参时会把该字符串
        /// 包成长度 1 的 <c>object[]</c>，使「无参」调用看起来带了 1 个参数。
        /// 本重载保证 <see cref="Args"/> 为 null，语义才是真的「无参」。
        /// </remarks>
        public static Issue Blocking(string key)
        {
            return new Issue(key, IssueSeverity.Error, null);
        }

        /// <summary>阻断级（带格式化参数）。</summary>
        public static Issue Blocking(string key, params object[] args)
        {
            return new Issue(key, IssueSeverity.Error, args);
        }

        /// <summary>
        /// 警告级：用户点「是」即可继续。仅用于「结果仍然可信，只是值得提醒」的情况。
        /// </summary>
        /// <remarks>无参重载的存在理由同 <see cref="Blocking(string)"/>。</remarks>
        public static Issue Advisory(string key)
        {
            return new Issue(key, IssueSeverity.Warning, null);
        }

        /// <summary>警告级（带格式化参数）。</summary>
        public static Issue Advisory(string key, params object[] args)
        {
            return new Issue(key, IssueSeverity.Warning, args);
        }
    }
}
