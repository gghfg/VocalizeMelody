using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Runs;

namespace VocalizeMelody;

/// <summary>
/// 判断“当前局内本地玩家是否为愚者”。只在愚者局内启用注入/改名。
/// 卡牌优先按 Owner 判断（战斗/手牌/奖励实例）；canonical（模板）卡不能读 Owner，
/// 直接回退到局内玩家判断。任何模型状态异常都不应影响 UI。
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
    /// 卡牌级判断：canonical 模板（如百科大全里的卡）不能访问 Owner，直接按局内判断；
    /// 其它实例优先按 Owner 的角色，拿不到再回退局内判断。
    /// </summary>
    public static bool ActiveFor(CardModel card)
    {
        if (card == null)
        {
            return false;
        }

        try
        {
            if (card.IsCanonical)
            {
                return Active;
            }

            var owner = card.Owner;
            if (owner != null)
            {
                return owner.Character is Fool;
            }
        }
        catch
        {
            // CanonicalModelException 等模型状态异常：回退到局内判断，绝不把异常抛给 UI。
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
