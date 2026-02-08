using ChainedPuzzles;
using HarmonyLib;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CP_Cluster_Hud))]
public static class CP_Cluster_HudPatch
{
    [HarmonyPatch(nameof(CP_Cluster_Hud.SetPlayerData))]
    [HarmonyPostfix]
    public static void SetPlayerData__Postfix(CP_Cluster_Hud __instance)
    {
        // Ensure wrapped hud has expanded character array
        var wrappedHud = __instance.m_bioscanHUDComp?.TryCast<CP_Bioscan_Hud>();

        if (wrappedHud != null && wrappedHud.m_progressBarPlayerChar.Length < PluginConfig.MaxPlayers)
            wrappedHud.m_progressBarPlayerChar = PluginConfig.GetBioscanLetters();
    }
}
