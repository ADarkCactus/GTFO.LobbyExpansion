using BepInEx.Unity.IL2CPP.Utils.Collections;
using GTFO.LobbyExpansion.Util;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(ElevatorRide))]
public static class ElevatorRidePatch
{
    // Why do the empty return true prefixes exist? Catching exceptions to not cause infinite drop-in bug.

    [HarmonyPatch(nameof(ElevatorRide.StartPreReleaseSequence))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static bool StartPreReleaseSequence__Prefix(Il2CppSystem.Action onDone)
    {
        L.LogExecutingMethod();
        return true;
    }

    [HarmonyPatch(nameof(ElevatorRide.Cleanup))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static bool Cleanup__Prefix()
    {
        L.LogExecutingMethod();
        return true;
    }

    [HarmonyPatch(nameof(ElevatorRide.SpawnShaftSegments))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void SpawnShaftSegments__Postfix(ElevatorRide __instance)
    {
        // Seems to be only run once on startup
        L.LogExecutingMethod();

        var shaftTop = __instance.m_shaftTop;

        var seats = shaftTop.m_elevatorSeats;

        // We need to extend this for other parts of the game's code.
        L.Verbose($"Expanding {nameof(__instance.m_shaftTop)}.{nameof(shaftTop.m_elevatorSeats)} size from {seats.Length} to {PluginConfig.MaxPlayers}.");

        shaftTop.m_elevatorSeats = new Il2CppReferenceArray<ElevatorSeat>(PluginConfig.MaxPlayers);

        for (var i = 0; i < PluginConfig.MaxPlayers; i++)
        {
            var seatIndex = i % 4;
            shaftTop.m_elevatorSeats[i] = seats[seatIndex];
        }

        // Needed for cage.ConnectToShaftTop() to not throw a NRE
        var loadingArmAnims = shaftTop.m_HSULoadingArmAnims;

        L.Verbose($"Expanding {nameof(__instance.m_shaftTop)}.{nameof(shaftTop.m_HSULoadingArmAnims)} size from {loadingArmAnims.Length} to {PluginConfig.MaxPlayers}.");

        shaftTop.m_HSULoadingArmAnims = new Il2CppReferenceArray<Animator>(PluginConfig.MaxPlayers);

        for (var i = 0; i < PluginConfig.MaxPlayers; i++)
        {
            var seatIndex = i % 4;
            shaftTop.m_HSULoadingArmAnims[i] = loadingArmAnims[seatIndex];
        }

        __instance.StartCoroutine(CoroutineHelpers.NextFrame(() =>
        {
            // If we don't delay this by a frame things are null for some reason,
            // even tho they're instantiated earlier in the original method
            // Il2Cpp moment I guess ... (or working with 3+ year old mono code as reference -moment)

            var cage = __instance.m_elevatorCage;

            var hsuAligns = cage.m_HSUAligns;

            L.Verbose($"Expanding {nameof(__instance.m_elevatorCage)}.{nameof(cage.m_HSUAligns)} size from {hsuAligns.Length} to {PluginConfig.MaxPlayers}.");

            cage.m_HSUAligns = new Il2CppReferenceArray<Transform>(PluginConfig.MaxPlayers);

            for (var i = 0; i < PluginConfig.MaxPlayers; i++)
            {
                var seatIndex = i % 4;
                cage.m_HSUAligns[i] = hsuAligns[seatIndex];
            }
        }).WrapToIl2Cpp());
    }

    [HarmonyPatch(nameof(ElevatorRide.Cleanup))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Cleanup__Postfix()
    {
        L.LogExecutingMethod();

        if (!ElevatorRide.ElevatorRideInProgress)
            return;

        ElevatorRide.ElevatorRideInProgress = false;
        ElevatorRide.Current.m_shaftTop.Cleanup();
    }
}
