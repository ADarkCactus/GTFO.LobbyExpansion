using HarmonyLib;
using SNetwork;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(SNet_Core))]
public static class SNet_CorePatch
{
    [HarmonyPatch(nameof(SNet_Core.GetBotNickname))]
    [HarmonyPostfix]
    public static void GetBotNickname__Postfix(ref string __result, int characterIndex)
    {
        if (characterIndex > 3)
            __result = PluginConfig.GetExtraSlotNickname(characterIndex);
    }
}
