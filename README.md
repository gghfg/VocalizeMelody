# 假如发声就有旋律？！（VocalizeMelody）

**愚者专属 Mod（Slay the Spire 2 v0.113.1 public-beta）**

让卡名中带「哇 / 啾 / 叽」同音字的 114 张卡牌真正成为旋律卡——点亮对应旋律、触发旋律联动，并把卡名显示成替换后的旋律字。

> 究极打击 → `啾 叽 打 叽`　究极防御 → `啾 叽 防御`　瓦解 → `哇 解`

[安装](#安装) · [兼容性](#兼容性) · [自行构建](#自行构建) · [License](#license)

## 特性

- **114 张卡逐张收录**：所有卡池（含其它角色卡、事件卡、Mod 卡）里名字带 wa / jiu / ji 同音字的卡，逐一记录在 [`src/MelodyTable.g.cs`](src/MelodyTable.g.cs)。
- **真正的旋律卡**：被收录的卡带 `Wah / Cho / Ki` 标签，出牌后点亮旋律，并联动 `Melodic Shot`、`Rhythm Strike`、`Steady Cadence`、`Harmonic Shield`、`Coda`、`Wah Symphony`、`Cho Chorus`、`Ki Verse`、`Ocarina`、`Bottled Song` 等。
- **卡名替换**：命中的字各自成一个 token，连续没命中的字合并成一个 token，token 之间一个空格；升级后缀 `+N` 保留。
- **音符特效**：打出时播放对应的 Wah / Cho / Ki 音符特效，每个旋律一次，与原版愚者卡一致。
- **只在愚者局内生效**：非愚者角色、主菜单、局外图鉴完全不改。

## 示例

| 原卡名 | 显示名 | 获得旋律 |
| --- | --- | --- |
| 究极打击 | `啾 叽 打 叽` | Cho + Ki |
| 究极防御 | `啾 叽 防御` | Cho + Ki |
| 瓦解 | `哇 解` | Wah |
| 打击 | `打 叽` | Ki |
| 铁蒺藜 | `铁 叽 藜` | Ki |

## 安装

### 创意工坊

在工坊订阅后启用即可（游戏需切到 **public-beta** 分支）。

### 手动安装

把以下两个文件放进 `<Slay the Spire 2>/mods/VocalizeMelody/`：

```text
VocalizeMelody.json
VocalizeMelody.dll
```

启动游戏，首次加载时允许 Mod，然后重启一次。游戏右下角出现 “Mods Loaded” 即成功。

## 兼容性

- 游戏版本：**v0.113.1（public-beta）**；更低版本没有旋律标签，Mod 会安全地不生效。
- 与 BaseLib、RitsuLib、常见换皮 / 界面类 Mod 共存。
- 多人游戏：需要双方安装同版本（本 Mod 标记为 `affects_gameplay`）。
- 与其它修改卡牌标题（`CardModel.Title`）的 Mod 可能冲突；如遇显示异常，先禁用其它改名类 Mod 排查。

## 已知限制

- 只处理**简体中文**卡名；其它语言保持原样（英文原卡名本来就带官方旋律标签）。
- 只在愚者局内生效；主菜单的卡牌图鉴不替换。
- 0.113.x 仍处于 beta，游戏更新可能改动相关 API；更新后请确认日志中的 `3 patched methods`。

## 自行构建

需要 .NET 9 SDK，以及游戏安装目录中的 `sts2.dll` / `0Harmony.dll`。

```powershell
dotnet build .\VocalizeMelody.csproj -c Release
```

默认游戏路径为 Steam 的 `C:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2`，可用 MSBuild 属性覆盖：

```powershell
dotnet build .\VocalizeMelody.csproj -c Release -p:Sts2Dir="D:\SteamLibrary\steamapps\common\Slay the Spire 2"
```

构建后会自动复制 DLL 与清单到 `mods\VocalizeMelody\`（目录不可写时只警告）。

### 维护替换表

替换表是逐张记录，不依赖拼音库。新增 / 删除卡牌时直接编辑 [`src/MelodyTable.g.cs`](src/MelodyTable.g.cs) 后重新构建即可。当前已排除原版旋律卡（`WAH_*` / `CHO_*` / `KI_*`）以及人工判定为误读的 `CLEANSE`。

## 参考

- 游戏内旋律实现：`MegaCrit.Sts2.Core.Commands.MelodyCmd` 与 `CardTag.Wah/Cho/Ki`（Slay the Spire 2 v0.113.1）。
- 旋律判定基于“本回合出牌历史 + 卡牌标签”，本 Mod 通过注入标签复用同一套逻辑。

## License

[MIT](LICENSE) © 2026 brooks
