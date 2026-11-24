using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace VocalizeMelody.Patches;

/// <summary>
/// 给目标卡追加旋律 tag。
/// 不修改 CardModel._tags 缓存本身，而是返回增强集合（并实例缓存），
/// 这样离开愚者局后不会留下“永久带旋律”的脏状态。
/// </summary>
[HarmonyPatch(typeof(CardModel), "get_Tags")]
internal static class CardTagsPatch
{
    private static readonly ConditionalWeakTable<CardModel, HashSet<CardTag>> Enhanced = new();

    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref IEnumerable<CardTag> __result)
    {
        if (!FoolRunContext.ActiveFor(__instance))
        {
            return;
        }

        MelodyTable.DiagnoseOnce();

        if (!MelodyTable.TryGet(__instance, out var entry))
        {
            return;
        }

        if (Enhanced.TryGetValue(__instance, out var cached))
        {
            __result = cached;
            return;
        }

        var set = new HashSet<CardTag>(__result ?? Array.Empty<CardTag>());
        foreach (var tag in entry.Tags)
        {
            set.Add(tag);
        }

        Enhanced.Add(__instance, set);
        __result = set;

        // 每张卡首次注入时打一条诊断日志，便于确认 Id.Entry 与表的匹配情况。
        Log.Info($"[{Entry.ModId}] inject {__instance.Id} -> {entry.Display} [{string.Join(",", entry.Tags)}]");
    }
}
