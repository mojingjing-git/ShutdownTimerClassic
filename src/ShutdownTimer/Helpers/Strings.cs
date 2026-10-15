using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ShutdownTimer.Helpers
{
    /// <summary>
    /// 文案查找表（i18n 基础设施）。
    ///
    /// 设计要点
    /// --------
    ///   * key 一律使用<b>英文原文</b>，也就是 <see cref="LocalizedItem.Value"/> 本身。
    ///     这样 key 和 <see cref="Timer.ExecutePowerAction"/> 的 switch case、settings.json 的
    ///     持久化值、命令行 <c>/SetAction</c> 参数天然对齐，不会随翻译而漂移。
    ///   * 查不到 key 时原样返回 key 本身，绝不抛异常：漏翻译最坏只是显示英文，不会让程序崩掉。
    ///   * 本轮（搭骨架）只登记了 <see cref="LocalizedCatalog"/> 里的 10 条 Display
    ///     （7 个电源动作 + 3 个托盘图标主题）。业务文案是下一步的事，暂时不要往这里塞。
    /// </summary>
    internal static class Strings
    {
        /// <summary>
        /// 简体中文文案表：key = 英文原文，value = 界面显示文本。
        /// </summary>
        private static readonly Dictionary<string, string> zhHans = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // 电源动作（7 条）
            { "Shutdown",       "关机" },
            { "Restart",        "重启" },
            { "Hibernate",      "休眠" },
            { "Sleep",          "睡眠" },
            { "Logout",         "注销" },
            { "Lock",           "锁定" },
            { "Custom Command", "自定义命令" },

            // 托盘图标主题（3 条）
            { "Automatic",      "自动" },
            { "Light",          "浅色" },
            { "Dark",           "深色" },

            // ===== 产品名（1 条）=====
            // 中文版沿用英文产品名，不翻译；登记成条目是为了让其它语言知道它可译。
            { "Shutdown Timer Classic", "Shutdown Timer Classic" },

            // ===== 对话框 / 窗体标题 =====
            { "Application already running!",                        "程序已在运行！" },
            { "Countdown Update",                                   "倒计时更新" },
            { "Invalid arguments",                                  "无效参数" },
            { "Invalid action",                                     "无效动作" },
            { "Invalid command!",                                   "无效命令！" },
            { "Invalid mode",                                       "无效模式" },
            { "Invalid time",                                       "无效时间" },
            { "Missing custom command",                             "缺少自定义命令" },
            { "Password Protection",                                "密码保护" },
            { "Set a new countdown",                                "设置新的倒计时" },
            { "Shutdown Timer Classic crashed and needs to be terminated!", "定时关机助手崩溃，需要终止！" },
            { "Start (with recommended settings)",                  "开始（使用推荐设置）" },
            { "Warning",                                            "警告" },

            // ===== 窗口标题 / 托盘菜单项 =====
            // 注意：以下若干 key 带首尾空格，是刻意与英文原文逐字对齐的结果。
            { "Are you sure?",                                     "确定要取消吗？" },
            { "Countdown",                                         "倒计时" },
            { "Countdown - ",                                      "倒计时 - " },
            { " - Shutdown Timer Classic",                         " - 定时关机助手" },
            { "Shutdown Timer - ",                                 "定时关机助手 - " },
            { " - Shutdown Timer Classic (paused)",             " - 定时关机助手（已暂停）" },
            { "Pause",                                             "暂停" },
            { "Resume",                                            "继续" },
            { "Do you really want to cancel the timer?",           "确定要取消本次计时吗？" },
            { "Would you like to re-lock the countdown?",          "是否要重新锁定倒计时界面？" },

            // ===== 气泡通知（托盘） =====
            { "Your timer was canceled successfully!\nThe application will now close.", "已成功取消本次计时！\n程序即将关闭。" },
            { "Timer has been moved to the background. Right-click the tray icon for more info.", "计时器已最小化到后台运行。右键点击托盘图标可查看更多信息。" },
            { "Timer has been reset. Remaining time until power action will be executed is {0} hours, {1} minutes and {2} seconds.", "计时已重置，距离执行电源动作还剩 {0} 小时 {1} 分钟 {2} 秒。" },
            { "2 hours remaining until the power action will be executed",   "距离执行电源动作还剩 2 小时" },
            { "1 hour remaining until the power action will be executed.",   "距离执行电源动作还剩 1 小时。" },
            { "30 minutes remaining until the power action will be executed.", "距离执行电源动作还剩 30 分钟。" },
            { "5 minutes remaining until the power action will be executed.",  "距离执行电源动作还剩 5 分钟。" },
            { "30 seconds remaining until the power action will be executed.", "距离执行电源动作还剩 30 秒。" },

            // ===== 倒计时对话框 =====
            { "Enter new time for the countdown in the format of HH:mm:ss or HH:mm.\n\nThis will replace the current timer in place.", "请按 HH:mm:ss 或 HH:mm 的格式输入新的倒计时时间。\n\n这将直接替换当前计时。" },
            { "Operation aborted: You have not supplied a new time value!", "操作已中止：你没有输入新的时间值！" },
            { "Operation aborted: You have not supplied a valid time value!", "操作已中止：你输入的时间值无效！" },
            { "Operation aborted: You have either not supplied a valid time value or there was an internal error outside the scope of your input while processing it.", "操作已中止：你输入的时间值无效，或在处理输入时发生了内部错误。" },

            // ===== 密码保护 =====
            { "This countdown has been protected with a password. Enter your password to release the lock.\nYou can re-lock the countdown by clicking on the lock icon afterwards.", "本次倒计时已启用密码保护。请输入密码以解除锁定。\n之后可通过点击锁形图标重新锁定倒计时界面。" },
            { "Enter your password to unlock this countdown.\n\nYou can re-lock the countdown by clicking on the lock icon afterwards.", "请输入密码以解锁本次倒计时。\n\n之后可通过点击锁形图标重新锁定倒计时界面。" },
            { "Incorrect password!\n\nThis countdown will stay locked until the correct password has been entered.", "密码错误！\n\n在输入正确密码之前，本次倒计时将保持锁定。" },

            // ===== 自定义命令 =====
            { "Custom command field was empty. Please enter a valid command or use a different action!", "自定义命令不能为空。请输入有效命令，或改用其它动作！" },
            { "There was an error executing your custom command.\n\nYour custom command: {0}\nError: {1}", "执行自定义命令时出错。\n\n自定义命令：{0}\n错误：{1}" },

            // ===== 多开检测 =====
            // 上游 Countdown 与 Menu 用的是同一句英文，但汉化时两者措辞不同
            // （"正在退出..." 与 "即将退出……"）。为保持界面零变化，这里拆成两个 key。
            { "Another instance of this application is already running. To allow multiple instances, please check the \"Allow multiple instances\" option in the application settings.\n\nExiting...", "本程序已在运行。若要允许多开，请在应用设置中勾选“允许多开”选项。\n\n正在退出..." },
            { "Another instance of this application is already running. To allow multiple instances, please check the \"Allow multiple instances\" option in the application settings.\n\nExiting... (Main menu)", "本程序已在运行。若要允许多开，请在应用设置中勾选“允许多开”选项。\n\n即将退出……" },

            // ===== 崩溃提示 =====
            { "The application ran into a critical error, leaving it in an unrecoverable state. As reliable operation can no longer be guaranteed, it will be terminated.\n\nA log file containing information about the process and the error has been saved to your desktop.\nPlease create an issue on GitHub and include the contents of this log file to help identify and fix the issue.\n\nGitHub: github.com/lukaslangrock/ShutdownTimerClassic/issues\nEmail: lukas.langrock@outlook.de", "程序遇到严重错误，已处于无法恢复的状态。由于无法再保证可靠运行，程序将被终止。\n\n包含进程信息与错误详情的日志文件已保存到你的桌面。\n请在 GitHub 上提交 issue，并附上该日志文件的内容，以便定位和修复问题。\n\nGitHub：github.com/mojingjing-git/ShutdownTimerClassic/issues" },

            // ===== Menu.cs 提示条（换行符必须与 key 完全一致）=====
            { "Applications that do not exit when prompted automatically get terminated by default to ensure a successful shutdown.\n\nA graceful shutdown, on the other hand, will wait for all applications to exit before continuing with the shutdown.\nThis might result in an unsuccessful shutdown if one or more applications are unresponsive or require a user interaction to exit.", "默认情况下，未响应关闭提示的程序会被强制结束，以确保关机顺利完成。\n\n而“安全关机”会等待所有程序自行退出后再继续关机。\n如果某个程序无响应，或需要用户交互才能退出，可能会导致关机失败。" },
            { "Depending on the power settings of your system, it might go to sleep after a certain amount of time due to inactivity.\nThis option will keep the system awake to ensure the timer can properly run and execute a shutdown.", "根据系统的电源设置，一段时间无操作后电脑可能会自动进入睡眠。\n启用此选项可让系统保持唤醒，确保计时器能正常运行并准时执行动作。" },
            { "This will launch the countdown without a visible window but will show a tray icon in your taskbar.", "启动倒计时后不显示窗口，仅在任务栏托盘显示一个图标。" },
            { "Will count down from the hours, minutes and seconds selected below,\nlike a countdown timer, and execute the power action when it reaches zero.", "从下方设置的时、分、秒开始倒数，\n就像普通计时器一样，归零后执行所选动作。" },
            { "In this mode you can select the target time of day (24h clock) for the power action.\nIf the time has already passed, it will roll over to tomorrow.\n\nWhen you press start, the appropriate countdown will be calculated.\n", "在此模式下可选择目标时刻（24 小时制）。\n若该时刻已过，将自动顺延至次日。\n\n点击开始后，程序会据此计算倒计时时长。\n" },

            // ===== Menu.cs RunChecks()（14 个新 key）=====
            // 这些 key 的 Args 一律传原始值（DateTimeOffset / double），
            // 格式说明写在 key 里，换行符必须与 key 完全一致。
            // "Warning" 已在上面的对话框标题一节登记，这里不重复。

            // ---------- 错误框的页眉 / 页脚 / 标题 ----------
            { "The following error(s) occurred:\n\n",                "发生以下错误：\n\n" },
            { "Please resolve the problem(s) and try again.",         "请解决上述问题后重试。" },
            { "There seems to be a problem!",                        "似乎出现了问题！" },

            // ---------- 警告框的收尾问句（标题 "Warning" 复用已有条目）----------
            { "Do you wish to continue?",                             "是否仍要继续？" },

            // ---------- 六个检查分支 ----------
            // B1：未选择动作
            { "Please select a valid action from the dropdown menu!", "请从下拉菜单中选择一个有效动作！" },
            // B2：倒计时模式从 0 开始
            { "The timer cannot start at 0 when in countdown mode!",  "倒计时模式下，计时不能从 0 开始！" },
            // B3：时间跨度超过 100 天（天数 / 年数传原始 double）
            { "Your chosen time equates to {0} days ({1} years)!\nThis is a bit much, and the application was never designed for this kind of runtime, so good luck!", "你设置的时间相当于 {0} 天（约 {1} 年）！\n这时间有点长，本程序从未针对如此长的运行时设计，祝你好运！" },
            // B4：时钟跳变——拆成 4 条，每条一个语义单元，译者才能调语序。
            // 注意第 2 条原本和第 3 条是紧挨着的两句（……之差计算。但计时结束时，……），
            // 拆开后会由 string.Join("\n\n", ...) 在两句之间多出一个空行，属于刻意接受的排版变化。
            { "Your local clock will shift during this countdown! The actual time the countdown will end on diverges from your target by {0} seconds, that's {1} hour(s). This can be caused by daylight savings transition or other scheduled events in your local timezone.", "倒计时期间你的本地时钟会发生跳变！实际结束时刻与你设定的目标相差 {0} 秒，约合 {1} 小时。这通常由夏令时切换或本地时区的其它时间调整事件引起。" },
            { "The countdown will use the difference between now and your target of {0:HH:mm:ss}.", "倒计时按当前时刻与你设定的 {0:HH:mm:ss} 之差计算。" },
            { "However, your clock will display {0:HH:mm:ss} when the timer ends.", "但计时结束时，你的时钟会显示 {0:HH:mm:ss}。" },
            { "The countdown will always use simple arithmetic instead of relying on timezone-aware calculation. Please be aware of this discrepancy!", "倒计时始终采用简单算术，而不会考虑时区规则，请留意这一差异！" },
            // B5：TimeSpan 换算失败。安全约束：severity 必须为 Error，详见 Menu.KeyTimeSpanConversionFailed。
            { "TimeSpan conversion failed! Please check if your time values are within a reasonable range or represent a valid time of day, if you used the time of day mode.", "时间跨度换算失败！请检查各时间值是否在合理范围内；如果你使用了定时模式，请确认所填时刻是否有效。" },
            // B6：权限不足——拆成两条独立 Issue，修复上游漏掉的 "\n\n"（否则两句话会连成一行）。
            { "Permission failure. Couldn't enable the required privliges for Shutdown/Restart/Logout from Windows. Error: {0}", "权限不足，无法向 Windows 申请关机/重启/注销所需的权限。错误信息：{0}" },
            { "To learn more about this issue, please go the application's settings and in the \"About\"-tab, click on \"Create Logfile\".", "如需了解此问题的详细信息，请进入应用设置页面，在“关于”选项卡中点击“创建日志文件”。" },

            // ===== Settings.cs =====
            { "Disabling the tray icon will prevent you from interacting with the application when it's running in the background.\nYour only choice in such a case is to kill the application with Task Manager.\n\nNotifications will also stop working!\n\nAre you sure you want to enable this option?", "禁用托盘图标后，程序在后台运行时将无法与其交互。\n遇到这种情况你只能用任务管理器强制结束程序。\n\n通知功能也会随之失效！\n\n确定要启用该选项吗？" },

            // ===== Program.cs 命令行参数错误 =====
            { "Failed to process command line arguments:\n\n{0}", "处理命令行参数失败：\n\n{0}" },
            { "'{0}' is not a valid action.\n\nPlease supply a valid action using the /SetAction argument.", "'{0}' 不是有效的动作。\n\n请使用 /SetAction 参数指定有效动作。" },
            { "The countdown requires a valid time greater than zero.\n\nPlease supply one using the /SetTime argument.", "倒计时需要大于零的有效时间。\n\n请使用 /SetTime 参数指定该时间。" },
            { "The action 'Custom Command' requires a command to execute.\n\nPlease supply one using the /SetCommand argument.", "“自定义命令”（Custom Command）动作需要指定要执行的命令。\n\n请使用 /SetCommand 参数指定该命令。" },
            { "'{0}' is not a valid control mode.\n\nValid modes are: Prefill, Lock, Launch, ForcedLaunch.", "'{0}' 不是有效的控制模式。\n\n有效模式为：Prefill、Lock、Launch、ForcedLaunch。" },

            // ===== 以下 key 来自 *.Designer.cs =====
            // 这些字符串在 Designer 里仍然保留着中文（设计时预览用），运行时由各窗体
            // 的 ApplyTexts() 用下面的查表结果覆盖，措辞与 Designer 里的中文逐字一致。
            // key 一律取自 upstream/master 的英文原文，未做任何改写。

            // ---------- Menu.Designer.cs（15 个 key / 17 处引用）----------
            { "Shutdown Timer", "定时关机助手" },
            { "The main window where you can choose the time and power action.", "主窗口：在此选择定时与电源动作。" },
            { "What to do?", "执行什么动作？" },
            { "When to do it?", "何时执行？" },
            { "Select an action:", "选择动作：" },
            { "At a specific time of day", "在指定的时刻" },
            { "After a specific timespan", "在指定的时长后" },
            { "Hours", "小时" },
            { "Minutes", "分钟" },
            { "Seconds", "秒" },
            { "Prevent system from going to sleep", "阻止系统进入睡眠" },
            { "Graceful (do not force close apps)", "安全关机（不强制关闭程序）" },
            { "Run in background", "在后台运行" },
            { "Start", "开始" },
            { "Help", "帮助" },

            // ---------- Settings.Designer.cs（44 个 key / 54 处引用）----------
            // 其中 9 个与 Menu 窗共用（Hours / Minutes / Seconds / Select an action: /
            // At a specific time of day / After a specific timespan / Prevent system from
            // going to sleep / Graceful (do not force close apps) / Run in background）。
            { "Shutdown Timer - Settings", "定时关机助手 - 设置" },
            { "Settings", "设置" },
            { "General", "常规" },
            { "Advanced", "高级" },
            { "About", "关于" },
            { "Made with love in Germany", "用心打造，德国制造" },
            { "View on GitHub", "在 GitHub 上查看" },

            // 常规页
            { "Application behaviour", "程序行为" },
            { "Allow multiple instances", "允许多开" },
            { "Remember last screen position (Countdown)", "记住倒计时窗口位置" },
            { "Remember last screen position (Menu)", "记住主窗口位置" },
            { "System tray menu", "系统托盘图标" },
            { "Trayicon theme:", "托盘图标主题：" },
            { "Clear Settings", "清除设置" },
            { "Timer defaults", "定时器默认值" },
            { "Custom defaults", "自定义默认值" },
            { "Remember last state", "记住上次状态" },

            // 高级页
            { "Developer Options", "开发者选项" },
            { "Open appdata folder", "打开 AppData 文件夹" },
            { "Save event logs to appdata on exit", "退出时将事件日志保存到 AppData" },
            { "Password protection", "密码保护" },
            { "Locks the countdown UI with a password you can set when starting a shutdown after clicking start.", "使用密码锁定倒计时界面，密码在开始关机后设置。" },
            { "Enable password protection", "启用密码保护" },
            { "Countdown window", "倒计时窗口" },
            { "Hide tray icon (not recommended)", "隐藏托盘图标（不推荐）" },
            { "Transparent countdown window", "透明倒计时窗口" },
            { "Adapt clock text to window size", "时钟文字自适应窗口大小" },
            { "(set background color)", "（设置背景颜色）" },
            { "Disable notifications and confirmations", "禁用通知与确认提示" },
            { "Disable animations", "禁用动画效果" },
            { "Disable always on top behaviour", "禁用窗口总在最前" },
            { "Force flag", "强制标志" },
            { "Changes how running applications are force-closed when\r\nusing a non-graceful shutdown.", "使用非安全方式关机时，将强制\r\n关闭正在运行的应用程序。" },
            { "Documentation", "文档" },

            // 关于页
            { "About this app", "关于本应用" },
            { "Create Logfile", "创建日志文件" },
            { "Email", "邮件联系" },
            { "GitHub Issues", "GitHub 问题反馈" },
            { "Licenses && Source", "许可与源码" },
            { "Application:", "应用程序：" },
            { "MIT License", "MIT 许可" },
            { "Sourcecode", "源代码" },
            // 注意：上游 key 结尾是半角冒号，中文版换成了全角冒号，措辞保持不变。
            { "Font Awesome:", "Font Awesome：" },
            { "CC BY 4.0 License", "CC BY 4.0 许可" },

            // ---------- Countdown.Designer.cs（7 个新 key / 12 处引用）----------
            // "Pause" / "Set a new countdown" / "Countdown" 已在上方登记，这里不重复。
            { "Shutdown Timer Countdown", "定时关机助手 - 倒计时" },
            { "Shows the time left until power action gets executed.", "显示距离执行电源动作的剩余时间。" },
            { "Stop and exit", "停止并退出" },
            { "Reset timer", "重置计时" },
            { "Restart application", "重启程序" },
            { "Move to background", "最小化到后台" },
            { "Show countdown window", "显示倒计时窗口" },

            // ---------- InputBox.Designer.cs（2 个 key / 2 处引用）----------
            { "OK", "确定" },
            { "Cancel", "取消" }
        };

        /// <summary>
        /// 取指定 key 的文案，必要时按 <paramref name="args"/> 做格式化。
        /// </summary>
        /// <param name="key">英文原文键；查不到时原样返回。</param>
        /// <param name="args">格式化参数；为空时<b>不</b>调用 <see cref="string.Format(string, object[])"/>。</param>
        public static string T(string key, params object[] args)
        {
            string t;
            if (!zhHans.TryGetValue(key, out t)) { t = key; WarnMissing(key); }
            if (args != null && args.Length > 0) { t = string.Format(t, args); }
            return t;
        }

        /// <summary>
        /// 漏翻译提示。<see cref="ConditionalAttribute"/> 让整个方法体在 Release 下被编译器剔除，
        /// 因此 Release 既不会有断言弹窗，也没有多余开销。
        /// </summary>
        [Conditional("DEBUG")]
        private static void WarnMissing(string key)
        {
            Debug.Assert(false, "缺少中文文案：" + key);
        }
    }
}
