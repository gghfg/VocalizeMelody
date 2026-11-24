using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace VocalizeMelody.Patches;

/// <summary>
/// 显示层：把命中同音字的卡名替换成 token 化的旋律名（保留升级后缀）。
/// 只在愚者局内、局内 UI/图鉴等场景生效；局外不动。
/// 任何异常都保持原值，避免影响卡牌库/奖励等 UI。
/// </summary>
[HarmonyPatch(typeof(CardModel), "get_Title")]
internal static class CardTitlePatch
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, ref string __result)
    {
        try
        {
            if (!FoolRunContext.ActiveFor(__instance))
            {
                return;
            }

            if (!MelodyTable.TryGet(__instance, out var entry))
            {
                return;
            }

            var title = __instance.TitleLocString;
            if (title == null)
            {
                __result = entry.Display;
                return;
            }

            var raw = title.GetFormattedText();
            if (string.IsNullOrEmpty(raw))
            {
                __result = entry.Display;
                return;
            }

            // get_Title 原始实现 = 本地化标题 + 升级后缀（"+" 或 "+N"）。
            // 用原始标题作前缀，保留后缀，避免升级显示丢失。
            if (__result != null && __result.StartsWith(raw, StringComparison.Ordinal))
            {
                __result = entry.Display + __result[raw.Length..];
            }
            else
            {
                __result = entry.Display;
            }
        }
        catch
        {
            // 保持原值
        }
    }
}
