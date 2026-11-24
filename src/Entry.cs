using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace VocalizeMelody;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public const string ModId = "VocalizeMelody";

    // ModLaunchManager / 兼容层可能多次调用 initializer；避免重复 PatchAll。
    private static bool _initialized;

    public static void Init()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        var harmony = new Harmony("sts2.vocalizemelody");
        harmony.PatchAll(typeof(Entry).Assembly);

        var patched = harmony.GetPatchedMethods().Count();
        Log.Info($"{ModId} initialized: {MelodyTable.Count} cards in table, {patched} patched methods");
    }
}
