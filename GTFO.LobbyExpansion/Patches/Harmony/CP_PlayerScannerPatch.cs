using ChainedPuzzles;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CP_PlayerScanner))]
public static class CP_PlayerScannerPatch
{
    [HarmonyPatch(nameof(CP_PlayerScanner.StartScan))]
    [HarmonyPrefix]
    public static bool StartScan__Prefix(CP_PlayerScanner __instance)
    {
        L.LogExecutingMethod();

        if (__instance.m_scanSpeeds.Length < PluginConfig.MaxPlayers)
        {
            L.Verbose($"Expanding {nameof(__instance.m_scanSpeeds)} size from {__instance.m_scanSpeeds.Length} to {PluginConfig.MaxPlayers} to account for more players being in the scan.");
            var original = __instance.m_scanSpeeds;
            __instance.m_scanSpeeds = new Il2CppStructArray<float>(PluginConfig.MaxPlayers);

            for (var i = 0; i < PluginConfig.MaxPlayers; i++)
                __instance.m_scanSpeeds[i] = original[Math.Min(i, original.Length - 1)];
        }

        return HarmonyControlFlow.Execute;
    }

    [HarmonyPatch(nameof(CP_PlayerScanner.CanGoFaster))]
    [HarmonyPrefix]
    public static bool CanGoFaster__Prefix(
        CP_PlayerScanner __instance,
        int nrOccupiers,
        ref bool __result)
    {
        // Clamp nrOccupiers to valid array bounds to prevent index out of range
        var maxIndex = __instance.m_scanSpeeds.Length - 1;

        if (__instance.m_reqItemsEnabled || nrOccupiers >= maxIndex || nrOccupiers >= PluginConfig.MaxPlayers)
        {
            __result = false;
            return HarmonyControlFlow.DontExecute;
        }

        __result = nrOccupiers == 0 || __instance.m_scanSpeeds[nrOccupiers - 1] < __instance.m_scanSpeeds[nrOccupiers];
        return HarmonyControlFlow.DontExecute;
    }
}
