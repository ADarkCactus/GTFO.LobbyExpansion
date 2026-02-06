using System.Collections.Generic;
using ChainedPuzzles;
using HarmonyLib;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CP_Bioscan_Hud))]
public static class CP_Bioscan_HudPatch
{
    // Map HUD instance ID to Core
    // stable int key instead of object reference
    internal static readonly Dictionary<int, CP_Bioscan_Core> HudToCoreMap = new();

    internal static void OnLevelCleanup()
    {
        HudToCoreMap.Clear();
    }

    [HarmonyPatch(nameof(CP_Bioscan_Hud.SetPlayerData))]
    [HarmonyPostfix]
    public static void SetPlayerData__Postfix(CP_Bioscan_Hud __instance)
    {
        if (__instance.m_progressBarPlayerChar.Length >= PluginConfig.MaxPlayers)
            return;

        __instance.m_progressBarPlayerChar = PluginConfig.GetBioscanLetters();
    }

    [HarmonyPatch(nameof(CP_Bioscan_Hud.OnDestroy))]
    [HarmonyPostfix]
    public static void OnDestroy__Postfix(CP_Bioscan_Hud __instance)
    {
        HudToCoreMap.Remove(__instance.GetInstanceID());
    }

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    public static void Update__Prefix(CP_Bioscan_Hud __instance)
    {
        if (!HudToCoreMap.TryGetValue(__instance.GetInstanceID(), out var bioscanCore))
            return;

        // Ensure bioscan core is valid
        if (bioscanCore == null || bioscanCore.WasCollected)
            return;

        var state = bioscanCore.m_sync.GetCurrentState();

        // Only update during active scan states
        if (state.status is eBioscanStatus.Disabled or eBioscanStatus.Finished or eBioscanStatus.TimedOut)
            return;

        // Override with authoritative count from sync state
        __instance.m_playersInScan = state.playersInScan;
        __instance.m_playersMax = state.playersMax;
    }
}
