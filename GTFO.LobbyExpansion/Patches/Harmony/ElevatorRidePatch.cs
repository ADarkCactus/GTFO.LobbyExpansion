using HarmonyLib;

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
