using System.Collections.Generic;
using ChainedPuzzles;
using HarmonyLib;
using Player;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CP_Bioscan_Core))]
public static class CP_Bioscan_CorePatch
{
    [HarmonyPatch(nameof(CP_Bioscan_Core.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(CP_Bioscan_Core __instance)
    {
        var hud = __instance.m_HUDComp?.TryCast<CP_Bioscan_Hud>();

        if (hud != null)
            CP_Bioscan_HudPatch.HudToCoreMap[hud.GetInstanceID()] = __instance;
    }

    [HarmonyPatch("OnSyncStateChange")]
    [HarmonyPostfix]
    public static void OnSyncStateChange__Postfix(
        CP_Bioscan_Core __instance,
        eBioscanStatus status,
        List<PlayerAgent> playersInScan,
        int playersMax)
    {
        // Get counts for logging/HUD fix
        var syncCount = __instance.m_sync.GetCurrentState().playersInScan;
        var listCount = playersInScan?.Count ?? 0;

        if (status != eBioscanStatus.Scanning)
            return;

        // Fix travel scan movement for >4 players
        if (__instance.IsMovable)
        {
            var movingComp = __instance.m_movingComp;

            if (movingComp != null && movingComp.OnlyMoveWhenScannig)
            {
                // Calculate true player count by checking positions ourselves
                // Don't trust sync state - it may be stale due to movement feedback loop
                var actualCount = CountPlayersInScan(__instance);

                var requireAll = __instance.m_playerScanner.ScanPlayersRequired.RequireAllPlayers();
                var requireSolo = __instance.m_playerScanner.ScanPlayersRequired.RequireSoloPlayer();
                var noRequirement = __instance.m_playerScanner.ScanPlayersRequired == PlayerRequirement.None;

                var shouldMove = noRequirement ||
                                 (requireAll && actualCount == playersMax) ||
                                 (requireSolo && actualCount == 1);

                if (shouldMove)
                    movingComp.ResumeMovement();
                else
                    movingComp.PauseMovement();
            }
        }

        // HUD fix: only needed when sync count exceeds list count
        if (syncCount <= listCount)
            return;

        var localPlayerInScan = false;

        if (__instance.enabled && playersInScan != null)
            for (var i = 0; i < listCount; i++)
                if (playersInScan[i] != null && playersInScan[i].IsLocallyOwned)
                {
                    localPlayerInScan = true;
                    break;
                }

        if (!localPlayerInScan && PlayerManager.TryGetLocalPlayerAgent(out var localPlayer) && localPlayer.Alive)
        {
            var scanner = __instance.m_PlayerScannerComp?.TryCast<CP_PlayerScanner>();

            if (scanner != null)
            {
                var radius = scanner.Radius;
                var distSqr = (localPlayer.Position - __instance.transform.position).sqrMagnitude;
                localPlayerInScan = distSqr < radius * radius;
            }
        }

        __instance.m_hud.SetPlayerData(
            __instance.m_puzzleIndex,
            syncCount,
            playersMax,
            localPlayerInScan,
            __instance.m_playerScanner.ScanPlayersRequired,
            __instance.m_playerScanner.ReduceWhenNoPlayer);
    }

    private static int CountPlayersInScan(CP_Bioscan_Core core)
    {
        var scanner = core.m_PlayerScannerComp?.TryCast<CP_PlayerScanner>();

        if (scanner == null)
            return 0;

        var radiusSqr = scanner.Radius * scanner.Radius;
        var scanPos = core.transform.position;
        var count = 0;
        var players = PlayerManager.PlayerAgentsInLevel;

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];

            if (player != null && player.Alive)
            {
                var distSqr = (player.Position - scanPos).sqrMagnitude;

                if (distSqr < radiusSqr)
                    count++;
            }
        }

        return count;
    }
}
