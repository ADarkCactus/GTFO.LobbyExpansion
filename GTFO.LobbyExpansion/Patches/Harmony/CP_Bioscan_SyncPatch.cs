using System.Collections.Generic;
using ChainedPuzzles;
using HarmonyLib;
using Player;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CP_Bioscan_Sync))]
public static class CP_Bioscan_SyncPatch
{
    [HarmonyPatch(nameof(CP_Bioscan_Sync.SetStateData))]
    [HarmonyPostfix]
    public static void SetStateData__Postfix(
        CP_Bioscan_Sync __instance,
        List<PlayerAgent> playersInScan)
    {
        if (playersInScan == null || playersInScan.Count <= 4)
            return;

        // The switch statement in SetStateData only handles cases 1-4
        // When count > 4, no player references are set, leaving stale data
        // Fill all 4 available struct slots with the first 4 players
        if (playersInScan[0]?.Owner != null)
            __instance.m_latestState.playerInScan1.SetPlayer(playersInScan[0].Owner);
        if (playersInScan[1]?.Owner != null)
            __instance.m_latestState.playerInScan2.SetPlayer(playersInScan[1].Owner);
        if (playersInScan[2]?.Owner != null)
            __instance.m_latestState.playerInScan3.SetPlayer(playersInScan[2].Owner);
        if (playersInScan[3]?.Owner != null)
            __instance.m_latestState.playerInScan4.SetPlayer(playersInScan[3].Owner);
    }
}
