using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Player;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(ElevatorCage))]
public static class ElevatorCagePatch
{
    // Why do the empty return true prefixes exist? Catching exceptions to not cause infinite drop-in bug.

    [HarmonyPatch(nameof(ElevatorCage.SkipPreReleaseSequence))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static bool SkipPreReleaseSequence__Prefix(ElevatorCage __instance, CellSoundPlayer sound, ref int playerId)
    {
        L.LogExecutingMethod($"{nameof(playerId)}: {playerId}");

        // We need to extend this for other parts of the game's code.
        // if (__instance.m_seatsFromShaft.Length < PluginConfig.MaxPlayers)
        // {
        //     L.Verbose($"Expanding {nameof(__instance.m_seatsFromShaft)} size from {__instance.m_seatsFromShaft.Length} to {PluginConfig.MaxPlayers}.");
        //     var original = __instance.m_seatsFromShaft;
        //     __instance.m_seatsFromShaft = new Il2CppReferenceArray<ElevatorSeat>(PluginConfig.MaxPlayers);
        //
        //     for (var x = 0; x < original.Length; x++)
        //     {
        //         var seat = x % 4; //x > 3 ? 0 : x;
        //         __instance.m_seatsFromShaft[x] = original[seat];
        //     }
        // }

        // if (playerId >= __instance.m_seatsFromShaft.Length)
        // {
        //     L.Warning($"playerId {playerId} is past seats from shaft length: skipping call.");
        //     return HarmonyControlFlow.DontExecute;
        // }

        if (__instance.m_seatsFromShaft[playerId] == null)
        {
            L.Warning($"playerId {playerId} has a null seat. Skipping call.");
            return HarmonyControlFlow.DontExecute;
        }

        return HarmonyControlFlow.Execute;
    }


    [HarmonyPatch(nameof(ElevatorCage.RegisterSpawnPoints))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static bool RegisterSpawnPoints__Prefix(ElevatorCage __instance)
    {
        L.LogExecutingMethod();
        __instance.m_spawnPoints = new Il2CppReferenceArray<PlayerSpawnpoint>(PluginConfig.MaxPlayers);

        for (var i = 0; i < PluginConfig.MaxPlayers; i++)
        {
            var seat = i % 4; //i > 3 ? 0 : i;
            __instance.m_spawnPoints[i] = PlayerManager.RegisterSpawnpoint(
                PlayerspawnpointType.StartInElevator,
                i,
                __instance.m_seatsFromShaft[seat]);
        }

        return HarmonyControlFlow.DontExecute;
    }

    [HarmonyPatch(nameof(ElevatorCage.PlaySeatOpenStraps))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static bool PlaySeatOpenStraps__Prefix(CellSoundPlayer sound, int playerId, bool isLocal)
    {
        if (playerId > 3)
        {
            L.Verbose("Skipping PlaySeatOpenStraps for playerId > 3.");
            return HarmonyControlFlow.DontExecute;
        }

        return HarmonyControlFlow.Execute;
    }
}
