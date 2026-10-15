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
            { "Dark",           "深色" }
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
