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
    ///   * 起步时只登记了 <see cref="LocalizedCatalog"/> 里的 10 条 Display（7 个电源动作 +
    ///     3 个托盘图标主题）；后续已把 7 个业务窗体与三个 Designer 的界面文案全部并入，
    ///     现有 <b>147</b> 条。新增界面文案一律登记到这里，不要再散回各个 .cs 硬编码。
    /// </summary>
    internal static class Strings
    {
        /// <summary>
        /// 「关于」页那段长文本的英文原文键。
        /// </summary>
        /// <remarks>
        /// <para>这段原本硬编码在 <c>Settings.resx</c> 的 <c>aboutRichTextBox.Text</c> 里，走的是
        /// .NET 资源机制、<b>不经过查表</b>，所以切到英文/日文界面时它仍然是中文——
        /// 这是上一轮只按「Designer 里的控件文案」盘点时漏掉的。</para>
        /// <para>现在纳入查表。resx 里的中文保留作设计时预览，运行时由
        /// <see cref="AboutText"/> 覆盖，与其它 Designer 文案同一套路。</para>
        /// <para>声明成常量而不是在两处各写一遍长字面量，是为了让字典 key 与调用点
        /// <b>不可能对不上</b>；拼接结果就是 key 本身，换行一律 <c>\r\n</c>。</para>
        /// </remarks>
        internal const string KeyAboutText =
              "This app is a free open-source project by Lukas Langrock under the MIT license which means there is no warranty provided and I shall not be held liable for any kind of damage!\r\n"
            + "After all you may lose unsaved work or mess up another running application. Use this with caution and common sense!\r\n"
            + "\r\n"
            + "The application is built to be reliable and should accurately measure and count time. I can't exactly guarantee this but I aim to get the application to be as stable and polished as possible.\r\n"
            + "In the case you encounter an error or notice the application not working as it should, an issue on GitHub or an Email would be very appreciated so I can fix the mistake.\r\n"
            + "\r\n"
            + "If you enjoy using this free app, please consider writing a review on the Microsoft Store or starring the project on GitHub.\r\n"
            + "Any contributions in the form of issues and PR's are very welcome. If you don't have a GitHub account, you can also write me an Email.";

        /// <summary>「关于」页长文本的简体中文译文。</summary>
        /// <remarks>
        /// <para>译文提成常量、而不是把多行 <c>+</c> 拼接直接写进字典，是因为实测集合初始化器里
        /// <b>跨行书写</b>的元素（<c>{ Key,</c> 换行后才闭合 <c>}</c>）末尾必须跟逗号，
        /// 否则编译报 <c>CS1003: 语法错误，应输入","</c>。挪到括号外拼接后，
        /// 字典恢复「一条一行」，与另外 149 条格式一致。</para>
        /// <para>声明成 <c>const</c> 而非 <c>static readonly</c>：常量在编译期求值，不依赖
        /// 静态字段的初始化顺序，日后有人把这段挪到字典定义之后也不会拿到 null。</para>
        /// </remarks>
        internal const string AboutTextZhHans =
              "本应用是 Lukas Langrock 开发的免费开源项目，采用 MIT 许可协议发布。\r\n"
            + "这意味着本程序不提供任何担保，作者不对任何形式的损失承担责任！\r\n"
            + "请注意：你可能会丢失未保存的工作，或干扰其它正在运行的程序。\r\n"
            + "请谨慎使用，并保持理性判断！\r\n"
            + "\r\n"
            + "本程序力求可靠，倒计时应当准确无误。我无法作出绝对保证，但目标是让它尽可能稳定、完善。\r\n"
            + "如果你遇到错误，或发现程序运行异常，欢迎在 GitHub 提交 issue 或发邮件给我，以便我修复问题。\r\n"
            + "\r\n"
            + "如果你喜欢这个免费应用，欢迎到 Microsoft Store 写条评价，或在 GitHub 上点个 star。\r\n"
            + "也欢迎以 issue 和 PR 的形式参与贡献。如果你没有 GitHub 账号，也可以直接发邮件给我。";

        /// <summary>「关于」页长文本的日语译文。结构与中文版逐行对应（4 + 空行 + 2 + 空行 + 2），行尾一律 <c>\r\n</c>、末尾无换行。</summary>
        /// <remarks>
        /// 用词与本文件术语对照表一致：<c>Issue</c> 大写（对齐下方错误弹窗条目）、
        /// 「メール」不译 Email、「スター」对应 GitHub star、第一人称统一「私」。
        /// </remarks>
        internal const string AboutTextJa =
              "本アプリケーションは Lukas Langrock が開発した無料のオープンソースプロジェクトで、MIT ライセンスのもとで公開されています。\r\n"
            + "これはいかなる保証も提供されないことを意味し、私はあらゆる種類の損害について責任を負いません！\r\n"
            + "いずれにせよ、保存されていない作業を失ったり、実行中の他のアプリケーションの動作を妨げたりする場合があります。\r\n"
            + "常識と節度をもって、慎重にお使いください！\r\n"
            + "\r\n"
            + "本アプリケーションは高い信頼性のために作られており、時間も正確に計測してカウントダウンできるはずです。ただしこれを完全に保証することはできませんが、本アプリが可能な限り安定で、完成度の高いものになるよう努めています。\r\n"
            + "エラーが発生したり、想定どおりに動作していないことに気付かれた場合は、不具合を修正するために GitHub に Issue を投稿するか、私にメールを送っていただければ非常に助かります。\r\n"
            + "\r\n"
            + "この無料アプリを気に入っていただけたら、Microsoft Store にレビューを書くか、GitHub でプロジェクトにスターをつけていただけると嬉しく思います。\r\n"
            + "Issue や PR による貢献も大歓迎です。GitHub アカウントをお持ちでない場合も、私にメールを送ってください。";

        /// <summary>「关于」页那段长文本，按当前语言查表。</summary>
        public static string AboutText()
        {
            return T(KeyAboutText);
        }

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
            // 注意：这条 key 不是上游英文原文。汉化时中文硬编码删掉了上游的
            // "A log file … saved to your desktop." 整句，也没保留末尾的 Email 行，
            // 并把仓库链接指向了本 fork。key 现状与既有中文一一对应，保证界面零变化；
            // 代价是将来若真加英文语言，fallback 出来的英文会比上游多一句、少一个 Email 行。
            // 到那一步以本 key 为准重新对齐上游原文即可，不影响中文版。
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
            // B3、B6 两条的 key 去掉了上游字面量尾部的 "\n\n"：段落分隔改由 Menu.Render()
            // 的 string.Join("\n\n") 统一负责，key 里再带一份就会多出空行。中文路径已验证等价；
            // 副作用是若 T() 将来回退到英文 key 显示，这两条会少一个段落间距。

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
            { "Cancel", "取消" },

            // ===== 语言选择器（Phase 5 新增）=====
            // "Interface language" 是 GroupBox 标题，"Language:" 是标签；
            // 注意它们不是上游英文原文，只服务于语言下拉框自己，
            // 因此英文档会原样显示（英文档不查表，key 即译文）。
            { "Interface language", "界面语言" },
            { "Language:", "语言：" },
            // 「关于」页长文本：key 用常量，保证与调用点逐字一致。
            // 译文见上方 AboutTextZhHans；文本原样取自 Settings.resx（未经手工转录）。
            { KeyAboutText, AboutTextZhHans }
        };

        // ============================================================================
        // 日本語（ja）文案表：key = 英文原文，value = 界面显示文本。
        //
        // 来源：src/ShutdownTimer/Helpers/Strings.cs 中的 zhHans 字典，共 147 条。
        // 本片段只翻译 value；key 逐字复制自源文件（含首尾空格、\n / \r\n 转义、
        // \" 转义双引号），条目数量与顺序与源文件完全一致。
        //
        // 翻译原则
        // --------
        //   * 语气统一：说明文、提示文、对话正文用「です・ます体」；
        //     按钮、菜单项、托盘项、设置项标签用名词或动词原形（短），不加句尾词。
        //   * 不添加中文里没有的敬语与语气词；不漏译、不增译。长句允许拆句或调整语序，
        //     但不改变含义。
        //   * 占位符 {0} / {1} / {2:HH:mm:ss} 的个数、顺序与语义位置，
        //     必须与 key 及中文译文三方一致。
        //   * 换行结构与 key 及中文译文完全一致，含 key 末尾的 \n 与 \r\n。
        //   * 产品名 Shutdown Timer / Shutdown Timer Classic 不翻译，沿用英文原文，
        //     与 zhHans 中 "Shutdown Timer Classic" 一条的处理保持一致。
        //   * UI 术语优先沿用 Windows 日语惯例（OK / エラー / 警告 / 詳細設定 等）。
        //
        // 术语对照表见本文件末尾注释。
        // ============================================================================

        private static readonly Dictionary<string, string> ja = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // 电源动作（7 条）
            { "Shutdown",       "シャットダウン" },
            { "Restart",        "再起動" },
            { "Hibernate",      "休止" },
            { "Sleep",          "スリープ" },
            { "Logout",         "ログアウト" },
            { "Lock",           "ロック" },
            { "Custom Command", "カスタムコマンド" },

            // 托盘图标主题（3 条）
            { "Automatic",      "自動" },
            { "Light",          "ライト" },
            { "Dark",           "ダーク" },

            // ===== 产品名（1 条）=====
            // 沿用英文产品名，不翻译；登记成条目是为了让其它语言知道它可译。
            { "Shutdown Timer Classic", "Shutdown Timer Classic" },

            // ===== 对话框 / 窗体标题 =====
            { "Application already running!",                        "アプリケーションは既に起動しています！" },
            { "Countdown Update",                                   "カウントダウンを更新" },
            { "Invalid arguments",                                  "無効な引数" },
            { "Invalid action",                                     "無効なアクション" },
            { "Invalid command!",                                   "無効なコマンド！" },
            { "Invalid mode",                                       "無効なモード" },
            { "Invalid time",                                       "無効な時間" },
            { "Missing custom command",                             "カスタムコマンドが未指定" },
            { "Password Protection",                                "パスワード保護" },
            { "Set a new countdown",                                "新しいカウントダウンを設定" },
            { "Shutdown Timer Classic crashed and needs to be terminated!", "Shutdown Timer Classic がクラッシュしたため、終了する必要があります！" },
            { "Start (with recommended settings)",                  "開始（推奨設定を使用）" },
            { "Warning",                                            "警告" },

            // ===== 窗口标题 / 托盘菜单项 =====
            // 注意：以下若干 key 带首尾空格，是刻意与英文原文逐字对齐的结果。
            { "Are you sure?",                                     "よろしいですか？" },
            { "Countdown",                                         "カウントダウン" },
            { "Countdown - ",                                      "カウントダウン - " },
            { " - Shutdown Timer Classic",                         " - Shutdown Timer Classic" },
            { "Shutdown Timer - ",                                 "Shutdown Timer - " },
            { " - Shutdown Timer Classic (paused)",             " - Shutdown Timer Classic（一時停止中）" },
            { "Pause",                                             "一時停止" },
            { "Resume",                                            "再開" },
            { "Do you really want to cancel the timer?",           "本当にタイマーをキャンセルしますか？" },
            { "Would you like to re-lock the countdown?",          "カウントダウンを再びロックしますか？" },

            // ===== 气泡通知（托盘） =====
            { "Your timer was canceled successfully!\nThe application will now close.", "タイマーを正常にキャンセルしました！\nアプリケーションを終了します。" },
            { "Timer has been moved to the background. Right-click the tray icon for more info.", "タイマーをバックグラウンドに移動しました。詳細についてはトレイアイコンを右クリックしてください。" },
            { "Timer has been reset. Remaining time until power action will be executed is {0} hours, {1} minutes and {2} seconds.", "タイマーをリセットしました。電源アクションの実行までの残り時間は {0} 時間 {1} 分 {2} 秒です。" },
            { "2 hours remaining until the power action will be executed",   "電源アクションの実行まで残り 2 時間です" },
            { "1 hour remaining until the power action will be executed.",   "電源アクションの実行まで残り 1 時間です。" },
            { "30 minutes remaining until the power action will be executed.", "電源アクションの実行まで残り 30 分です。" },
            { "5 minutes remaining until the power action will be executed.",  "電源アクションの実行まで残り 5 分です。" },
            { "30 seconds remaining until the power action will be executed.", "電源アクションの実行まで残り 30 秒です。" },

            // ===== 倒计时对话框 =====
            { "Enter new time for the countdown in the format of HH:mm:ss or HH:mm.\n\nThis will replace the current timer in place.", "新しいカウントダウン時間を HH:mm:ss または HH:mm の形式で入力してください。\n\n現在のタイマーがそのまま置き換えられます。" },
            { "Operation aborted: You have not supplied a new time value!", "操作を中止しました：新しい時間値が入力されていません！" },
            { "Operation aborted: You have not supplied a valid time value!", "操作を中止しました：入力された時間値が無効です！" },
            { "Operation aborted: You have either not supplied a valid time value or there was an internal error outside the scope of your input while processing it.", "操作を中止しました：入力された時間値が無効であるか、入力の処理中に内部エラーが発生しました。" },

            // ===== 密码保护 =====
            { "This countdown has been protected with a password. Enter your password to release the lock.\nYou can re-lock the countdown by clicking on the lock icon afterwards.", "このカウントダウンはパスワードで保護されています。ロックを解除するにはパスワードを入力してください。\nその後、ロックアイコンをクリックするとカウントダウンを再ロックできます。" },
            { "Enter your password to unlock this countdown.\n\nYou can re-lock the countdown by clicking on the lock icon afterwards.", "このカウントダウンのロックを解除するにはパスワードを入力してください。\n\nその後、ロックアイコンをクリックするとカウントダウンを再ロックできます。" },
            { "Incorrect password!\n\nThis countdown will stay locked until the correct password has been entered.", "パスワードが正しくありません！\n\n正しいパスワードが入力されるまで、このカウントダウンはロックされたままになります。" },

            // ===== 自定义命令 =====
            { "Custom command field was empty. Please enter a valid command or use a different action!", "カスタムコマンドが入力されていません。有効なコマンドを入力するか、別のアクションを指定してください！" },
            { "There was an error executing your custom command.\n\nYour custom command: {0}\nError: {1}", "カスタムコマンドの実行中にエラーが発生しました。\n\nカスタムコマンド：{0}\nエラー：{1}" },

            // ===== 多开检测 =====
            // 上游 Countdown 与 Menu 用的是同一句英文，但译文里两者措辞不同
            // （"終了しています..." 与 "終了します……"）。为保持界面零变化，这里拆成两个 key。
            { "Another instance of this application is already running. To allow multiple instances, please check the \"Allow multiple instances\" option in the application settings.\n\nExiting...", "このアプリケーションは既に別のインスタンスで実行中です。複数のインスタンスを許可するには、アプリケーションの設定で「複数のインスタンスを許可」オプションをオンにしてください。\n\n終了しています..." },
            { "Another instance of this application is already running. To allow multiple instances, please check the \"Allow multiple instances\" option in the application settings.\n\nExiting... (Main menu)", "このアプリケーションは既に別のインスタンスで実行中です。複数のインスタンスを許可するには、アプリケーションの設定で「複数のインスタンスを許可」オプションをオンにしてください。\n\n終了します……" },

            // ===== 崩溃提示 =====
            // 注意：这条 key 不是上游英文原文，是中文硬编码改写后的版本（少一句
            // "A log file … saved to your desktop."、少一个 Email 行，链接指向本 fork）。
            // 日语沿用同一口径：段落切分、GitHub 地址与中文版保持一致。
            { "The application ran into a critical error, leaving it in an unrecoverable state. As reliable operation can no longer be guaranteed, it will be terminated.\n\nA log file containing information about the process and the error has been saved to your desktop.\nPlease create an issue on GitHub and include the contents of this log file to help identify and fix the issue.\n\nGitHub: github.com/lukaslangrock/ShutdownTimerClassic/issues\nEmail: lukas.langrock@outlook.de", "アプリケーションで重大なエラーが発生し、回復できない状態になりました。安定した動作を保証できないため、強制終了されます。\n\nプロセス情報とエラーの詳細を含むログファイルがデスクトップに保存されました。\nGitHub に Issue を作成し、このログファイルの内容を添付してください。問題の特定と修正に役立てます。\n\nGitHub：github.com/mojingjing-git/ShutdownTimerClassic/issues" },

            // ===== Menu.cs 提示条（换行符必须与 key 完全一致）=====
            { "Applications that do not exit when prompted automatically get terminated by default to ensure a successful shutdown.\n\nA graceful shutdown, on the other hand, will wait for all applications to exit before continuing with the shutdown.\nThis might result in an unsuccessful shutdown if one or more applications are unresponsive or require a user interaction to exit.", "既定では、終了の確認に応答せず自動的に終了しないアプリケーションは、シャットダウンを確実に完了させるため強制終了されます。\n\n一方、「安全なシャットダウン」は、すべてのアプリケーションが終了するまで待ってからシャットダウンを続行します。\n1 つ以上のアプリケーションが応答しない場合や、終了にユーザーの操作が必要な場合は、シャットダウンが正常に行われない可能性があります。" },
            { "Depending on the power settings of your system, it might go to sleep after a certain amount of time due to inactivity.\nThis option will keep the system awake to ensure the timer can properly run and execute a shutdown.", "システムの電源設定によっては、一定時間操作がない場合にスリープ状態になることがあります。\nこのオプションを有効にするとシステムが起きたままになり、タイマーが正常に動作してシャットダウンを実行できるようになります。" },
            { "This will launch the countdown without a visible window but will show a tray icon in your taskbar.", "可視ウィンドウを表示せずにカウントダウンを開始し、タスクバーのトレイにアイコンだけを表示します。" },
            { "Will count down from the hours, minutes and seconds selected below,\nlike a countdown timer, and execute the power action when it reaches zero.", "以下で選択した時間・分・秒を基準にカウントダウンし、\n通常のカウントダウンタイマーのように 0 になると電源アクションを実行します。" },
            { "In this mode you can select the target time of day (24h clock) for the power action.\nIf the time has already passed, it will roll over to tomorrow.\n\nWhen you press start, the appropriate countdown will be calculated.\n", "このモードでは、電源アクションを実行する時刻（24 時間表示）を指定できます。\nその時刻を過ぎている場合は、自動的に翌日に繰り越されます。\n\n開始を押すと、それに合わせたカウントダウンが計算されます。\n" },

            // ===== Menu.cs RunChecks()（14 个新 key）=====
            // 这些 key 的 Args 一律传原始值（DateTimeOffset / double），
            // 格式说明写在 key 里，换行符必须与 key 完全一致。
            // "Warning" 已在上面的对话框标题一节登记，这里不重复。

            // ---------- 错误框的页眉 / 页脚 / 标题 ----------
            { "The following error(s) occurred:\n\n",                "次のエラーが発生しました：\n\n" },
            { "Please resolve the problem(s) and try again.",         "問題を解決してからもう一度お試しください。" },
            { "There seems to be a problem!",                        "問題が発生したようです！" },

            // ---------- 警告框的收尾问句（标题 "Warning" 复用已有条目）----------
            { "Do you wish to continue?",                             "続行しますか？" },

            // ---------- 六个检查分支 ----------
            // B1：未选择动作
            { "Please select a valid action from the dropdown menu!", "ドロップダウンメニューから有効なアクションを選択してください！" },
            // B2：倒计时模式从 0 开始
            { "The timer cannot start at 0 when in countdown mode!",  "カウントダウンモードでは、タイマーを 0 から開始できません！" },
            // B3：时间跨度超过 100 天（天数 / 年数传原始 double）
            { "Your chosen time equates to {0} days ({1} years)!\nThis is a bit much, and the application was never designed for this kind of runtime, so good luck!", "指定した時間は {0} 日間（約 {1} 年間）に相当します！\nかなり長い時間です。このアプリケーションはこのような実行時間を想定して設計されていないため、その点をご了承ください。" },
            // B4：时钟跳变——拆成 4 条，每条一个语义单元，译者才能调语序。
            { "Your local clock will shift during this countdown! The actual time the countdown will end on diverges from your target by {0} seconds, that's {1} hour(s). This can be caused by daylight savings transition or other scheduled events in your local timezone.", "このカウントダウン中にローカル時刻がずれます！実際のカウントダウン終了時刻は目標時刻から {0} 秒（{1} 時間）ずれています。夏時間への切り替えなど、ローカルのタイムゾーンにおけるその他の時間変更が原因となることがあります。" },
            { "The countdown will use the difference between now and your target of {0:HH:mm:ss}.", "カウントダウンは、現在時刻と目標時刻 {0:HH:mm:ss} との差から計算されます。" },
            { "However, your clock will display {0:HH:mm:ss} when the timer ends.", "ただし、タイマーが終了すると、時計には {0:HH:mm:ss} と表示されます。" },
            { "The countdown will always use simple arithmetic instead of relying on timezone-aware calculation. Please be aware of this discrepancy!", "カウントダウンは常に単純な計算を使用し、タイムゾーンを考慮した計算は行いません。この差異にご注意ください！" },
            // B5：TimeSpan 换算失败。安全约束：severity 必须为 Error，详见 Menu.KeyTimeSpanConversionFailed。
            { "TimeSpan conversion failed! Please check if your time values are within a reasonable range or represent a valid time of day, if you used the time of day mode.", "時間間隔の変換に失敗しました！各時間値が妥当な範囲にあるかを確認し、時刻指定モードを使用している場合は有効な時刻かどうかも確認してください。" },
            // B6：权限不足——拆成两条独立 Issue，段落分隔由 Menu.Render() 的 string.Join("\n\n") 负责。
            { "Permission failure. Couldn't enable the required privliges for Shutdown/Restart/Logout from Windows. Error: {0}", "権限エラー。シャットダウン/再起動/ログアウトに必要な権限を Windows から取得できませんでした。エラー：{0}" },
            { "To learn more about this issue, please go the application's settings and in the \"About\"-tab, click on \"Create Logfile\".", "この問題の詳細については、アプリケーションの設定を開き、「このアプリについて」タブで「ログファイルを作成」をクリックしてください。" },

            // ===== Settings.cs =====
            { "Disabling the tray icon will prevent you from interacting with the application when it's running in the background.\nYour only choice in such a case is to kill the application with Task Manager.\n\nNotifications will also stop working!\n\nAre you sure you want to enable this option?", "トレイアイコンを無効にすると、バックグラウンドで実行中のアプリケーションを操作できなくなります。\nその場合、アプリケーションを終了させるにはタスクマネージャーから強制終了するしかありません。\n\n通知も機能しなくなります！\n\nこのオプションを有効にしてよろしいですか？" },

            // ===== Program.cs 命令行参数错误 =====
            { "Failed to process command line arguments:\n\n{0}", "コマンドライン引数の処理に失敗しました：\n\n{0}" },
            { "'{0}' is not a valid action.\n\nPlease supply a valid action using the /SetAction argument.", "'{0}' は有効なアクションではありません。\n\n/SetAction 引数で有効なアクションを指定してください。" },
            { "The countdown requires a valid time greater than zero.\n\nPlease supply one using the /SetTime argument.", "カウントダウンには 0 より大きい有効な時間が必要です。\n\n/SetTime 引数で指定してください。" },
            { "The action 'Custom Command' requires a command to execute.\n\nPlease supply one using the /SetCommand argument.", "「カスタムコマンド」アクションには、実行するコマンドの指定が必要です。\n\n/SetCommand 引数で指定してください。" },
            { "'{0}' is not a valid control mode.\n\nValid modes are: Prefill, Lock, Launch, ForcedLaunch.", "'{0}' は有効な制御モードではありません。\n\n有効なモード：Prefill、Lock、Launch、ForcedLaunch" },

            // ===== 以下 key 来自 *.Designer.cs =====
            // key 一律取自 upstream/master 的英文原文，未做任何改写。

            // ---------- Menu.Designer.cs（15 个 key / 17 处引用）----------
            { "Shutdown Timer", "Shutdown Timer" },
            { "The main window where you can choose the time and power action.", "時間と電源アクションを選択できるメインウィンドウです。" },
            { "What to do?", "何をしますか？" },
            { "When to do it?", "いつ実行しますか？" },
            { "Select an action:", "アクションを選択：" },
            { "At a specific time of day", "指定した時刻" },
            { "After a specific timespan", "指定した時間の経過後" },
            { "Hours", "時間" },
            { "Minutes", "分" },
            { "Seconds", "秒" },
            { "Prevent system from going to sleep", "システムがスリープ状態になるのを防止する" },
            { "Graceful (do not force close apps)", "安全な終了（アプリケーションを強制終了しない）" },
            { "Run in background", "バックグラウンドで実行" },
            { "Start", "開始" },
            { "Help", "ヘルプ" },

            // ---------- Settings.Designer.cs（44 个 key / 54 处引用）----------
            { "Shutdown Timer - Settings", "Shutdown Timer - 設定" },
            { "Settings", "設定" },
            { "General", "一般" },
            { "Advanced", "詳細" },
            { "About", "情報" },
            { "Made with love in Germany", "ドイツで、愛を込めて制作" },
            { "View on GitHub", "GitHub で表示" },

            // 常规页
            { "Application behaviour", "アプリケーションの動作" },
            { "Allow multiple instances", "複数のインスタンスを許可" },
            { "Remember last screen position (Countdown)", "直前のウィンドウ位置を記憶（カウントダウン）" },
            { "Remember last screen position (Menu)", "直前のウィンドウ位置を記憶（メインウィンドウ）" },
            { "System tray menu", "システムトレイメニュー" },
            { "Trayicon theme:", "トレイアイコンのテーマ：" },
            { "Clear Settings", "設定を消去" },
            { "Timer defaults", "タイマーの既定値" },
            { "Custom defaults", "カスタムの既定値" },
            { "Remember last state", "直前の状態を記憶" },

            // 高级页
            { "Developer Options", "開発者オプション" },
            { "Open appdata folder", "appdata フォルダーを開く" },
            { "Save event logs to appdata on exit", "終了時にイベントログを appdata に保存する" },
            { "Password protection", "パスワード保護" },
            { "Locks the countdown UI with a password you can set when starting a shutdown after clicking start.", "パスワードを使ってカウントダウン画面をロックします。パスワードは、「開始」をクリックしてシャットダウンを開始するときに設定できます。" },
            { "Enable password protection", "パスワード保護を有効にする" },
            { "Countdown window", "カウントダウンウィンドウ" },
            { "Hide tray icon (not recommended)", "トレイアイコンを非表示にする（非推奨）" },
            { "Transparent countdown window", "カウントダウンウィンドウを透明にする" },
            { "Adapt clock text to window size", "時計のテキストをウィンドウサイズに合わせる" },
            { "(set background color)", "（背景色を設定）" },
            { "Disable notifications and confirmations", "通知と確認メッセージを無効にする" },
            { "Disable animations", "アニメーションを無効にする" },
            { "Disable always on top behaviour", "常に手前に表示する機能を無効にする" },
            { "Force flag", "強制フラグ" },
            { "Changes how running applications are force-closed when\r\nusing a non-graceful shutdown.", "安全でないシャットダウンを行うときに、実行中のアプリケーションを\r\n強制終了する方法を変更します。" },
            { "Documentation", "ドキュメント" },

            // 关于页
            { "About this app", "このアプリについて" },
            { "Create Logfile", "ログファイルを作成" },
            { "Email", "メール" },
            { "GitHub Issues", "GitHub Issues" },
            { "Licenses && Source", "ライセンスとソースコード" },
            { "Application:", "アプリケーション：" },
            { "MIT License", "MIT ライセンス" },
            { "Sourcecode", "ソースコード" },
            // 注意：上游 key 结尾是半角冒号，日语版沿用中文版的处理，换成全角冒号，措辞保持不变。
            { "Font Awesome:", "Font Awesome：" },
            { "CC BY 4.0 License", "CC BY 4.0 ライセンス" },

            // ---------- Countdown.Designer.cs（7 个新 key / 12 处引用）----------
            { "Shutdown Timer Countdown", "Shutdown Timer - カウントダウン" },
            { "Shows the time left until power action gets executed.", "電源アクションの実行までの残り時間を表示します。" },
            { "Stop and exit", "停止して終了" },
            { "Reset timer", "タイマーをリセット" },
            { "Restart application", "アプリケーションを再起動" },
            { "Move to background", "バックグラウンドに移動" },
            { "Show countdown window", "カウントダウンウィンドウを表示" },

            // ---------- InputBox.Designer.cs（2 个 key / 2 处引用）----------
            { "OK", "OK" },
            { "Cancel", "キャンセル" },
            { "Interface language", "インターフェース言語" },
            { "Language:", "言語：" },

            // 「关于」页长文本：与中文版同一条 key（两语言的 key 都是上游英文原文），译文见上方 AboutTextJa。
            { KeyAboutText, AboutTextJa }
        };

        // ============================================================================
        // 术语对照表（全文统一用词，改文案时请同步改这里）
        //
        //   Shutdown / shutdown ............ シャットダウン（电源动作）
        //   Restart ....................... 再起動
        //   Hibernate ..................... 休止
        //   Sleep .......................... スリープ
        //   Logout ........................ ログアウト
        //   Lock / lock / re-lock ......... ロック
        //   Custom Command ................. カスタムコマンド
        //   power action .................. 電源アクション（全文不用「電源操作」）
        //   countdown ..................... カウントダウン
        //   countdown window .............. カウントダウンウィンドウ
        //   timer ........................ タイマー
        //   Start / Pause / Resume ........ 開始 / 一時停止 / 再開
        //   Cancel / cancel ............... キャンセル（取消计时 = タイマーをキャンセル）
        //   OK ............................ OK
        //   Error / Warning ............... エラー / 警告
        //   Settings / General / Advanced . 設定 / 全般 / 詳細設定
        //   About / About this app ........ このアプリについて
        //   Help / Documentation .......... ヘルプ / ドキュメント
        //   application behaviour ........ アプリケーションの動作
        //   tray icon ..................... トレイアイコン
        //   tray icon theme ............... トレイアイコンのテーマ
        //   system tray menu .............. システムトレイメニュー
        //   taskbar ....................... タスクバー
        //   main window ................... メインウィンドウ
        //   log / log file ................ ログ / ログファイル
        //   event log ..................... イベントログ
        //   Create Logfile ................ ログファイルを作成
        //   Issue (GitHub) ................ Issue
        //   password protection ........... パスワード保護
        //   graceful (shutdown) ........... 安全（…方式） / 安全なシャットダウン
        //   non-graceful shutdown ......... 安全でないシャットダウン
        //   force close / force-closed .... 強制終了
        //   Force flag .................... 強制フラグ
        //   notification .................. 通知
        //   confirmation .................. 確認メッセージ
        //   hour(s) / minute(s) / second(s) 時間 / 分 / 秒
        //   clock / local clock ........... 時計 / ローカル時刻
        //   daylight savings .............. 夏時間
        //   timezone ...................... タイムゾーン
        //   multiple instances ............ 複数のインスタンス
        //   license ....................... ライセンス
        //   source code ................... ソースコード
        //   (not recommended) ............. （非推奨）
        //
        //   专有名词不翻译：Shutdown Timer、Shutdown Timer Classic、Font Awesome、
        //   GitHub、MIT、CC BY 4.0、TimeSpan、Prefill / Lock / Launch / ForcedLaunch、
        //   /SetAction / /SetTime / /SetCommand、HH:mm:ss / HH:mm、appdata
        // ============================================================================

        /// <summary>
        /// 取指定 key 的文案，<b>不做任何格式化</b>。
        /// </summary>
        /// <remarks>
        /// 这个重载不是冗余，它是决策 A 那条防护真正落地的地方。
        /// <c>params object[]</c> 有个反直觉的语义：只传一个字符串实参（如 <c>T("a {0}")</c>）时，
        /// 编译器会把它包成<b>长度 1</b> 的 <c>object[]</c>，于是下面那个重载里的
        /// <c>args.Length == 0</c> 判断<b>永远不成立</b>，无参调用照样会进
        /// <see cref="string.Format(string, object[])"/>——而原文含花括号时会抛
        /// <see cref="System.FormatException"/>，正是决策 A 要防的那种崩法。
        /// <br />
        /// 有了本重载，无参调用精确匹配到这里，永远走不到 <see cref="string.Format"/>。
        /// 对不含占位符的 key 结果完全一致（<c>string.Format</c> 本就恒等），属于纯粹的修正。
        /// </remarks>
        /// <param name="key">英文原文键；查不到时原样返回。</param>
        public static string T(string key)
        {
            return Lookup(key);
        }

        /// <summary>
        /// 取指定 key 的文案，必要时按 <paramref name="args"/> 做格式化。
        /// </summary>
        /// <param name="key">英文原文键；查不到时原样返回。</param>
        /// <param name="args">格式化参数；为空时<b>不</b>调用 <see cref="string.Format(string, object[])"/>。</param>
        public static string T(string key, params object[] args)
        {
            string t = Lookup(key);
            if (args != null && args.Length > 0) { t = string.Format(t, args); }
            return t;
        }

        /// <summary>
        /// 当前界面语言。<b>这里是唯一真源</b>，<see cref="Localization.Current"/> 转发到本属性。
        /// </summary>
        /// <remarks>
        /// 文案与字体必须永远看到同一个语言值，所以只在这里存一份。
        /// 改动请走 <see cref="Helpers.Localization.Parse"/> / <see cref="Helpers.SettingsData.Language"/>。
        /// </remarks>
        public static Language CurrentLanguage { get; set; } = Language.ZhHans;

        /// <summary>
        /// 取指定语言对应的文案表；返回 null 表示<b>该语言不查表</b>。
        /// </summary>
        /// <remarks>
        /// 英文档返回 null 是有意的：key 本身就是上游英文原文，不查表等于"原样显示"，
        /// 所以英文不需要维护第二份文案，零翻译成本也永远不会漏译。
        /// </remarks>
        private static Dictionary<string, string> TableFor(Language lang)
        {
            switch (lang)
            {
                case Language.ZhHans:
                    return zhHans;
                case Language.Ja:
                    return ja;
                default:
                    return null;
            }
        }

        /// <summary>按当前语言查表；查不到就回落英文原文（key 本身）。</summary>
        private static string Lookup(string key)
        {
            Dictionary<string, string> table = TableFor(CurrentLanguage);
            if (table == null)
            {
                return key;
            }

            string t;
            if (!table.TryGetValue(key, out t)) { t = key; WarnMissing(key); }
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
