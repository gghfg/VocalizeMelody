using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace VocalizeMelody.Patches;

/// <summary>
/// 给被注入旋律 tag 的卡补播旋律音符特效。
/// 原版只有 Fool 卡在自己的 OnPlay 里调用 MelodyCmd.PlayMelodyVfx；
/// 这里在 Hook 分发到“被打出的卡自身”的 AfterCardPlayed 时补一次（每张卡每个 tag 一次）。
/// </summary>
[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterCardPlayed))]
internal static class CardPlayedMelodyVfxPatch
{
    [HarmonyPostfix]
    private static void Postfix(AbstractModel __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        try
        {
            var card = cardPlay?.Card;
            if (card == null || !ReferenceEquals(__instance, card))
            {
                return;
            }

            if (!FoolRunContext.ActiveFor(card))
            {
                return;
            }

            if (!MelodyTable.TryGet(card, out var entry))
            {
                return;
            }

            var combat = CombatManager.Instance;
            if (combat != null && combat.IsOverOrEnding)
            {
                return;
            }

            var creature = card.Owner?.Creature ?? cardPlay?.Player?.Creature;
            if (creature is null)
            {
                return;
            }

            foreach (var tag in entry.Tags)
            {
                // 与原版 Fool 卡一致：每个旋律 tag 播一次音符特效。
                _ = MelodyCmd.PlayMelodyVfx(creature, tag);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"[{Entry.ModId}] melody vfx failed: {ex}");
        }
    }
}
