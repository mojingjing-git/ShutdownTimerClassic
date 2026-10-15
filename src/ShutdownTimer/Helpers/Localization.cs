using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ShutdownTimer.Helpers
{
    /// <summary>
    /// 可选界面语言。<c>Default</c> 表示"跟随 Designer 原样，不做任何字体替换"。
    /// </summary>
    internal enum Language
    {
        Default,
        ZhHans,
        Ja,
        Ko
    }

    /// <summary>
    /// 语言/字体应用器（i18n 基础设施）。
    ///
    /// 背景
    /// ----
    /// 这个 fork 的 <c>*.Designer.cs</c> 里，有 <b>8 处控件</b>被显式写死了
    /// <c>Microsoft YaHei UI</c>（标题 Label、按钮、托盘菜单项等），另外 4 个窗体还在
    /// <c>Load</c> 里硬写了 <c>this.Font</c>。字体一旦要跟着语言切换，这些硬编码就都是障碍。
    ///
    /// 设计要点
    /// --------
    ///   * <b>只换字体族，不碰其它排版属性。</b>各控件原有的 size / style / unit / charset
    ///     必须原样保留，否则版面会走样。
    ///   * <b>只替换族名等于 <see cref="TargetFamily"/> 的控件。</b>这既是"哪些控件该被汉化字体
    ///     接管"的判据，也自动保护了 <c>Countdown.Designer.cs</c> 里那个上游刻意用
    ///     <c>Verdana 24F Bold</c> 的倒计时数字标签——等宽数字是设计要求，不能动。
    ///   * <b>没显式设过字体的控件不碰。</b>它们通过 <see cref="Form.Font"/> 的 ambient 传播
    ///     自然拿到新字体，不需要、也不应该被强制钉死成实例字体。
    /// </summary>
    internal static class Localization
    {
        /// <summary>
        /// Designer 里硬编码的字体族名，同时是本轮唯一允许被替换的族名。
        /// </summary>
        private const string TargetFamily = "Microsoft YaHei UI";

        // 探测结果缓存为 static 字段：Font 是 GDI 对象，每次调用都 new 会泄漏句柄。
        private static readonly Font zhHansFont = Probe(new[] { "Microsoft YaHei UI", "Microsoft YaHei", "SimHei" });
        private static readonly Font jaFont = Probe(new[] { "Yu Gothic UI", "Meiryo UI", "MS UI Gothic" });
        private static readonly Font koFont = Probe(new[] { "Malgun Gothic" });

        /// <summary>
        /// 当前语言。本轮固定为 <see cref="Language.ZhHans"/>；接入真正的语言选择时再改成读配置。
        /// </summary>
        public static Language Current
        {
            get { return Language.ZhHans; }
        }

        /// <summary>
        /// 把当前语言的字体应用到 <paramref name="form"/>：先递归替换已有硬编码字体，
        /// 再设置窗体级 <see cref="Form.Font"/> 让未显式设字体的控件继承到新字体。
        /// </summary>
        public static void ApplyTo(Form form)
        {
            if (form == null)
            {
                return;
            }

            Font font = FontFor(Current);
            if (font == null)
            {
                return;
            }

            // 顺序很重要：必须先在"原始字体状态"下遍历，才能靠引用比较分辨控件是不是继承来的。
            ReplaceFont(form, font);
            form.Font = font;
        }

        /// <summary>
        /// 取指定语言对应的字体；<see cref="Language.Default"/> 返回 null，表示保持 Designer 原样。
        /// </summary>
        private static Font FontFor(Language lang)
        {
            switch (lang)
            {
                case Language.ZhHans:
                    return zhHansFont;
                case Language.Ja:
                    return jaFont;
                case Language.Ko:
                    return koFont;
                default:
                    return null;
            }
        }

        /// <summary>
        /// 按候选顺序探测字体族，返回第一个本机可用的字体；全部不可用时返回 null。
        /// </summary>
        private static Font Probe(string[] candidates)
        {
            if (candidates == null)
            {
                return null;
            }

            foreach (string name in candidates)
            {
                try
                {
                    // 用构造来探测：字体不存在时 GDI+ 会抛 ArgumentException。
                    // 不要用 new Font(name).FontFamily.Name 读回来比对，那样读到的是回退后的族名，
                    // 会把"字体缺失"误判成"字体可用"。
                    FontFamily family = new FontFamily(name);
                    return new Font(family, 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
                catch (ArgumentException)
                {
                    // 本机没装这个字体，试下一个候选。
                }
            }

            return null;
        }

        /// <summary>
        /// 递归处理控件树：替换显式写死为 <see cref="TargetFamily"/> 的字体，并下钻到
        /// 控件自己的上下文菜单里。
        /// </summary>
        private static void ReplaceFont(Control control, Font font)
        {
            if (!InheritsFont(control) && MatchesTarget(control.Font))
            {
                control.Font = Derive(font, control.Font);
            }

            ReplaceToolStrip(control.ContextMenuStrip, font);

            foreach (Control child in control.Controls)
            {
                ReplaceFont(child, font);
            }
        }

        /// <summary>
        /// 处理上下文菜单。菜单条目是 <see cref="ToolStripItem"/> 而非子控件，
        /// 只遍历 <see cref="Control.Controls"/> 够不到它们。
        /// </summary>
        private static void ReplaceToolStrip(ToolStrip strip, Font font)
        {
            if (strip == null)
            {
                return;
            }

            foreach (ToolStripItem item in strip.Items)
            {
                ReplaceToolStripItem(item, strip.Font, font);
            }
        }

        private static void ReplaceToolStripItem(ToolStripItem item, Font parentFont, Font font)
        {
            if (!ReferenceEquals(item.Font, parentFont) && MatchesTarget(item.Font))
            {
                item.Font = Derive(font, item.Font);
            }

            ToolStripDropDownItem dropDown = item as ToolStripDropDownItem;
            if (dropDown != null && dropDown.HasDropDownItems)
            {
                foreach (ToolStripItem child in dropDown.DropDown.Items)
                {
                    ReplaceToolStripItem(child, dropDown.DropDown.Font, font);
                }
            }
        }

        /// <summary>
        /// 控件是否没有显式设置字体（即 <see cref="Control.Font"/> 只是父级的同一个对象）。
        /// 这类控件交给 <see cref="Form.Font"/> 的 ambient 传播，不在这里钉死。
        ///
        /// 注意：顶层 <see cref="Form"/> 的 <see cref="Control.Parent"/> 为 null，
        /// 这里会对它返回 false。这是有意的——Form 的字体本来就是我们要设置的目标，
        /// 让它进入 <see cref="MatchesTarget"/> 的检查是安全的（见 <see cref="ApplyTo"/>）。
        /// </summary>
        private static bool InheritsFont(Control control)
        {
            return ReferenceEquals(control.Font, control.Parent == null ? null : control.Parent.Font);
        }

        /// <summary>
        /// 字体族是否是需要替换的目标族。Verdana 等刻意设计的字体会因此自动跳过。
        ///
        /// 为什么拿 <see cref="FontFamily.Name"/> 和英文串比对是安全的：
        /// GDI+ 返回的是本地化名，但只有"纯中文本地化字体"（宋体 SimSun、黑体 SimHei 等）
        /// 才以中文名注册；YaHei UI / Yu Gothic UI / Meiryo UI / Malgun Gothic 这类
        /// 带官方英文名的字体族，在任何语言的 Windows 上读回来的都是英文名。
        /// 已实测确认：<c>new FontFamily("Microsoft YaHei UI").Name == "Microsoft YaHei UI"</c>，
        /// 而 <c>new FontFamily("Microsoft YaHei").Name == "微软雅黑"</c>。
        /// 若将来要新增目标族，务必先按同样方式实测。
        /// </summary>
        private static bool MatchesTarget(Font font)
        {
            return font != null && string.Equals(font.FontFamily.Name, TargetFamily, StringComparison.Ordinal);
        }

        /// <summary>
        /// 派生字体缓存。键包含族名/字号/字形/单位/字符集，命中则复用同一个 Font 实例。
        ///
        /// 为什么必须缓存：<see cref="ApplyTo"/> 在每个窗体的 Load 都会调用，而 Settings 窗是
        /// 「每次点开都 new 一个」——不做缓存的话，每次开关设置窗都会为 titleLabel / footerLabel
        /// 各 new 一个 GDI Font 对象且永不释放，长期使用会累积句柄。
        /// 缓存的 Font 被多个控件共享是安全的：控件不负责 Dispose 赋给它的 Font。
        /// </summary>
        private static readonly Dictionary<string, Font> derivedCache = new Dictionary<string, Font>(StringComparer.Ordinal);

        /// <summary>
        /// 保留原有字号/字形/单位/字符集，只把字体族换成 <paramref name="font"/> 的族。
        /// </summary>
        private static Font Derive(Font font, Font existing)
        {
            string key = font.FontFamily.Name + "" + existing.Size.ToString(System.Globalization.CultureInfo.InvariantCulture)
                       + "" + (int)existing.Style
                       + "" + (int)existing.Unit
                       + "" + (int)existing.GdiCharSet;

            Font cached;
            if (derivedCache.TryGetValue(key, out cached))
            {
                return cached;
            }

            Font derived = new Font(font.FontFamily, existing.Size, existing.Style, existing.Unit, existing.GdiCharSet);
            derivedCache[key] = derived;
            return derived;
        }
    }
}
