using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Runs;

namespace VocalizeMelody;

/// <summary>
/// 判断“当前局内本地玩家是否为愚者”。只在愚者局内启用注入/改名。
/// 卡牌优先按 Owner 判断（战斗/手牌/奖励实例），没有 Owner 时回退到局内玩家。
/// 结果按 500ms 缓存，避免 get_Tags / get_Title 高频调用时反复查玩家。
/// </summary>
internal static class FoolRunContext
{
    private const int CacheMs = 500;
    private static bool _active;
    private static bool _logged;
    private static long _lastCheck = long.MinValue;

    public static bool Active
    {
        get
        {
            long now = Environment.TickCount64;
            if (now - _lastCheck < CacheMs)
            {
                return _active;
            }

            _lastCheck = now;
            _active = Compute();
            if (!_logged)
            {
                _logged = true;
                LogState();
            }

            return _active;
        }
    }

    /// <summary>
    /// 卡牌级判断：有 Owner 时以 Owner 的角色为准；否则（卡池/图鉴模板）用局内玩家。
    /// </summary>
    public static bool ActiveFor(CardModel card)
    {
        var owner = card.Owner;
        if (owner != null)
        {
            return owner.Character is Fool;
        }

        return Active;
    }

    private static bool Compute()
    {
        var runManager = RunManager.Instance;
        if (runManager == null || !runManager.IsInProgress)
        {
            return false;
        }

        var runState = runManager.DebugOnlyGetState();
        if (runState == null)
        {
            return false;
        }

        var me = LocalContext.GetMe(runState.Players);
        return me != null && me.Character is Fool;
    }

    private static void LogState()
    {
        try
        {
            var runManager = RunManager.Instance;
            var runState = runManager?.DebugOnlyGetState();
            var me = runState == null ? null : LocalContext.GetMe(runState.Players);
            Log.Info($"[{Entry.ModId}] fool-run check: active={_active}, inRun={runManager?.IsInProgress == true}, " +
                     $"players={runState?.Players.Count ?? -1}, me={(me == null ? "null" : me.Character?.Id?.Entry ?? "?")}");
        }
        catch (Exception ex)
        {
            Log.Error($"[{Entry.ModId}] fool-run check log failed: {ex}");
        }
    }
}
