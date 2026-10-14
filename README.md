# Shutdown Timer Classic 中文版

## ⚠️ 本仓库是第三方 Fork，非原创项目

| 项目 | 内容 |
|---|---|
| **上游原创项目** | <https://github.com/lukaslangrock/ShutdownTimerClassic> |
| **原作者 / 版权归属** | **Lukas Langrock** |
| **上游许可证** | MIT —— [LICENSE](LICENSE) 原封未改 |
| **本仓库基于** | 上游 `master` 分支提交 `22ac336`（2026-10-13） |
| **本仓库的改动** | 仅两项：界面汉化 + 高 DPI 配置修复（详见下方「相对上游的改动」） |
| **上游英文说明** | [README.upstream.md](README.upstream.md) |

本仓库保留了上游的**完整 git 历史**（237 个上游提交，起点 2019-10-02），原作者的版权声明、LICENSE、控件命名与代码归属均未作任何改动。本仓库建立在原作者的劳动成果之上，**所有原始设计功劳归于 Lukas Langrock**；本地化仅是增加了一层中文界面与一处配置修复。

如果你需要未汉化的原版，请前往上游仓库或 Microsoft Store 获取。

---

## 这是什么

一个轻量的 Windows 定时关机工具：设定时间，到点执行关机 / 重启 / 休眠 / 睡眠 / 注销 / 锁定 / 自定义命令。

上游版本仅有英文界面。本仓库提供完整的简体中文界面，并修复了上游在高 DPI 缩放下的一处配置冲突。

## 相对上游的改动

### 1. 界面汉化（完整覆盖）

主界面、倒计时窗口、托盘右键菜单、设置页三个页签、输入框、全部对话框与气泡通知，以及设置页 About 中的说明文本，全部翻译为简体中文。

界面字体由 `Microsoft Sans Serif` 替换为**微软雅黑 UI**，避免中文字形回退造成的排版异常。

### 2. 显示值与内部值分离

这是汉化过程中最需要小心的地方。

上游把下拉框的显示文本直接当作业务逻辑的内部值使用：

- `Timer.ExecutePowerAction` 用 `switch` 精确匹配英文 `"Shutdown"` / `"Restart"` / `"Hibernate"` / `"Sleep"` / `"Logout"` / `"Lock"` / `"Custom Command"`
- 托盘主题下拉框被 `Countdown.cs` 拿来比较 `"Light"` / `"Dark"`
- 设置文件（JSON）中持久化的同样是这些英文字符串
- 命令行参数也使用英文动作名

**如果直接翻译下拉框文字，关机动作会全部落入 `default` 分支而根本不执行，已有配置文件也会失效。**

因此本版本新增 `src/ShutdownTimer/LocalizedCatalog.cs`：下拉框显示中文，内部值保持英文（通过重写 `ToString()` 实现），代码侧统一使用 `GetValue()` / `SelectValue()` 读写英文值。共调整 12 处调用点，两处 `switch` 分支和所有 JSON 属性名一字未动。

> 结果：**界面是中文的，配置文件格式与命令行行为与英文版完全兼容。**

### 3. 高 DPI（Per-Monitor V2）修复

上游存在一处配置冲突：

- `App.config` 声明了 `DpiAwareness = PerMonitorV2`
- 但 `app.manifest` 只声明了旧式的 `<dpiAware>true</dpiAware>`（System DPI aware）
- 且从未设置 `EnableWindowsFormsHighDpiAutoResizing`

manifest 会在进程启动时将其标记为 System DPI aware，之后的 PerMonitorV2 请求**无法再升级进程**。实际结果是 Windows 对整个应用做位图拉伸缩放，在 125% / 150% / 175% / 200% 缩放下表现为**文字发虚、控件错位**。

本版本：

- `app.manifest` → `true/pm` 加上 Windows 10 1703+ 的 `<dpiAwareness>PerMonitorV2, PerMonitor</dpiAwareness>`
- `App.config` → 增加 `EnableWindowsFormsHighDpiAutoResizing`

该项修复已单独向上游提交 PR：<https://github.com/lukaslangrock/ShutdownTimerClassic/pull/65>

### 4. 目标框架

`v4.8` → `v4.8.1`。4.8.1 是 4.8 的服务更新版，与 Windows 11 22H2 及以后自带的运行时一致。

## AI 参与声明

**本仓库的本地化与修复工作由 AI 编码助手（MiniMax M3.1）完成。**

具体涵盖：代码结构分析、字符串耦合关系排查、本地化映射层设计、全部界面文本翻译、高 DPI 问题定位与修复，以及本文件的撰写。人类维护者审阅了最终改动并承担责任。

上游作者未参与本仓库的工作，亦未为其背书。原项目作者为 Lukas Langrock。

## 构建方式

需要安装 **.NET Framework 4.8.1 Developer Pack**（仅运行时不够）。

```powershell
# 还原依赖并编译 Release
msbuild src\ShutdownTimer\ShutdownTimer.csproj /restore /p:Configuration=Release
```

产物位于 `src\ShutdownTimer\bin\Release\`。其中 `ShutdownTimerClassic.exe`、`Newtonsoft.Json.dll` 与 `ShutdownTimerClassic.exe.config` **三者必须放在同一目录**才能运行。

## 已知限制

- **未做代码签名**：自编译产物没有微软商店的数字签名。首次运行时 Windows SmartScreen 可能提示风险，选择「更多信息 → 仍要运行」即可。本地运行不受影响。
- **无法通过 Microsoft Store 分发**：本版本不包含商店打包资源，也不满足商店认证要求。
- **数据安全**：上游 README 明确说明，该应用默认会强制关闭仍在运行的程序，正在保存的数据可能丢失。勾选「安全关机（不强制关闭程序）」可以避免这一点，但若某程序无响应则可能导致关机失败。
- **AI 生成**：如上所述，界面文本与代码改动由 AI 生成，未经上游作者审阅。

## 与上游的关系

- 基于上游 `master` 分支的 `22ac336`（2026-10-13）
- 上游仓库：<https://github.com/lukaslangrock/ShutdownTimerClassic>
- 上游采用 MIT 许可，允许 fork、修改与再分发，本仓库保留原作者版权声明
- 同步上游更新的方式（仓库已配置好 remote，无需手动添加）：

```powershell
# origin   -> 本仓库的 GitHub fork（用于推送）
# upstream -> lukaslangrock/ShutdownTimerClassic（官方上游）
git fetch upstream
git rebase upstream/master
```

汉化改动集中在界面文本与配置文件，冲突面很小。若上游将来自行加入多语言支持，可优先考虑改用其方案。

## 许可证

MIT。版权归 **Lukas Langrock** 所有。完整条款见 [LICENSE](LICENSE)。
