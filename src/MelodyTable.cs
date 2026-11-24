using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace VocalizeMelody;

internal sealed class MelodyEntry
{
    public string Display { get; }
    public CardTag[] Tags { get; }

    public MelodyEntry(string display, CardTag[] tags)
    {
        Display = display;
        Tags = tags;
    }
}

internal static partial class MelodyTable
{
    private static Dictionary<string, MelodyEntry>? _byNormalized;
    private static bool _diagnosed;

    public static int Count => Entries.Count;

    /// <summary>
    /// 依次尝试：模型 Entry、运行时类型名、以及忽略大小写/下划线/连字符的规范化名称。
    /// 兼容不同版本里 Id.Entry 的命名差异；同时防御 Id 尚未初始化的卡牌实例。
    /// </summary>
    public static bool TryGet(CardModel card, out MelodyEntry entry)
    {
        var modelId = card.Id;
        var entryName = modelId?.Entry;
        var typeName = card.GetType().Name;

        if (!string.IsNullOrEmpty(entryName) && Entries.TryGetValue(entryName, out entry!))
        {
            return true;
        }

        if (Entries.TryGetValue(typeName, out entry!))
        {
            return true;
        }

        var normalized = ByNormalized;
        if (!string.IsNullOrEmpty(entryName) && normalized.TryGetValue(Normalize(entryName), out entry!))
        {
            return true;
        }

        if (normalized.TryGetValue(Normalize(typeName), out entry!))
        {
            return true;
        }

        entry = null!;
        return false;
    }

    /// <summary>首次进入愚者局后做一次全卡表匹配自检，便于确认 Id.Entry 与表键一致。</summary>
    public static void DiagnoseOnce()
    {
        if (_diagnosed)
        {
            return;
        }

        _diagnosed = true;
        try
        {
            int total = 0, matched = 0;
            var samples = new List<string>();
            foreach (var card in ModelDb.AllCards)
            {
                total++;
                if (TryGet(card, out _))
                {
                    matched++;
                }
                else if (samples.Count < 3)
                {
                    samples.Add($"{card.Id} ({card.GetType().Name})");
                }
            }

            Log.Info($"[{Entry.ModId}] table self-check: {matched}/{total} cards matched; " +
                     $"sample misses: {string.Join(", ", samples)}");
        }
        catch (Exception ex)
        {
            Log.Error($"[{Entry.ModId}] table self-check failed: {ex}");
        }
    }

    private static Dictionary<string, MelodyEntry> ByNormalized => _byNormalized ??= BuildNormalized();

    private static Dictionary<string, MelodyEntry> BuildNormalized()
    {
        var dict = new Dictionary<string, MelodyEntry>(StringComparer.Ordinal);
        foreach (var pair in Entries)
        {
            dict[Normalize(pair.Key)] = pair.Value;
        }

        return dict;
    }

    private static string Normalize(string value)
    {
        return value.Replace("_", string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
    }
}
