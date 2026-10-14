using ShutdownTimer.Helpers;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ShutdownTimer
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                ExceptionHandler.Log("Register exception handler");
                AppDomain.CurrentDomain.UnhandledException += ExceptionHandler.UnhandledExceptionHandler;
                Application.ThreadException += ExceptionHandler.ThreadExceptionHandler;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length != 0)
            {
                ExceptionHandler.Log("Processing CLI args");
                try
                {
                    ArgProcessor.ProcessArgs(args);
                }
                catch (Exception ex)
                {
                    ExceptionHandler.Log("Failed to process CLI args: " + ex.Message);
                    MessageBox.Show("处理命令行参数失败：\n\n" + ex.Message, "无效参数", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Initialize settings
                ExceptionHandler.Log("Load settings");
                if (ArgProcessor.argNoSettings)
                { SettingsProvider.TemporaryMode = true; }
                SettingsProvider.Load();
                ExceptionHandler.Log("Settings loaded");

                string[] validActions = { "Shutdown", "Restart", "Hibernate", "Sleep", "Logout", "Lock", "Custom Command" };
                if (Array.IndexOf(validActions, ArgProcessor.argAction) == -1)
                {
                    ExceptionHandler.Log("Launch aborted: invalid /SetAction argument");
                    MessageBox.Show($"'{ArgProcessor.argAction}' 不是有效的动作。\n\n请使用 /SetAction 参数指定有效动作。", "无效动作", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                switch (ArgProcessor.argMode)
                {
                    case "Prefill":
                    case "Lock":
                        ExceptionHandler.Log("Run menu with args");

                        Menu menu = new Menu
                        {
                            ApplyArgumentValues = true,
                            ArgTimeH = ArgProcessor.argTimeH,
                            ArgTimeM = ArgProcessor.argTimeM,
                            ArgTimeS = ArgProcessor.argTimeS,
                            ArgAction = ArgProcessor.argAction,
                            ArgMode = ArgProcessor.argMode,
                            ArgPassword = ArgProcessor.argPassword,
                            ArgCommand = ArgProcessor.argCommand,
                            ArgGraceful = ArgProcessor.argGraceful,
                            ArgPreventSleep = ArgProcessor.argPreventSleep,
                            ArgBackground = ArgProcessor.argBackground,
                            ArgUseTimeOfDay = ArgProcessor.argUseTimeOfDay
                        };
                        Application.Run(menu);
                        ExceptionHandler.CreateAutoLogIfApplicable();
                        return;

                    case "Launch":
                    case "ForcedLaunch":
                        bool forced = !ArgProcessor.argMode.Equals("Launch");

                        if (string.IsNullOrWhiteSpace(ArgProcessor.argTime) || ArgProcessor.argTimeTS <= TimeSpan.Zero)
                        {
                            ExceptionHandler.Log("Launch aborted: time is missing, zero, or invalid");
                            MessageBox.Show("倒计时需要大于零的有效时间。\n\n请使用 /SetTime 参数指定该时间。", "无效时间", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // A custom command action requires a command to execute
                        if (ArgProcessor.argAction.Equals("Custom Command") && string.IsNullOrWhiteSpace(ArgProcessor.argCommand))
                        {
                            ExceptionHandler.Log("Launch aborted: action is 'Custom Command' but no /SetCommand argument was supplied");
                            MessageBox.Show("“自定义命令”（Custom Command）动作需要指定要执行的命令。\n\n请使用 /SetCommand 参数指定该命令。", "缺少自定义命令", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        ExceptionHandler.Log("Run Countdown with args");
                        Timer.CountdownTimeSpan = ArgProcessor.argTimeTS;
                        Timer.Action = ArgProcessor.argAction;
                        Timer.Command = ArgProcessor.argCommand;
                        Timer.Graceful = ArgProcessor.argGraceful;
                        Timer.PreventSystemSleep = ArgProcessor.argPreventSleep;
                        Timer.Start(ArgProcessor.argPassword, !ArgProcessor.argBackground, forced, false);
                        Application.Run();
                        ExceptionHandler.CreateAutoLogIfApplicable();
                        return;

                    default:
                        if (ArgProcessor.argMode != null)
                        {
                            ExceptionHandler.Log("Launch aborted: invalid /SetMode argument");
                            MessageBox.Show($"'{ArgProcessor.argMode}' 不是有效的控制模式。\n\n有效模式为：Prefill、Lock、Launch、ForcedLaunch。", "无效模式", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                }
            }

            ExceptionHandler.Log("Start menu");
            if (!SettingsProvider.SettingsLoaded)
            {
                SettingsProvider.Load();
            }
            Application.Run(new Menu());
            ExceptionHandler.CreateAutoLogIfApplicable();
        }
    }
}
