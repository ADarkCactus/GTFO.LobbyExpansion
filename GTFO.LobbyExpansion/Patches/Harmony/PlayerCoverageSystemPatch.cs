using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using PlayerCoverage;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(PlayerCoverageSystem))]
public static class PlayerCoverageSystemPatch
{
    [HarmonyPatch(nameof(PlayerCoverageSystem.Awake))]
    [HarmonyPostfix]
    public static void Awake__Postfix()
    {
        L.LogExecutingMethod();

        if (PlayerCoverageSystem.s_coverageKeys.Length < PluginConfig.MaxPlayers)
        {
            L.Verbose($"Expanding PlayerCoverageSystem::s_coverageKeys size from {PlayerCoverageSystem.s_coverageKeys.Length} to {PluginConfig.MaxPlayers}.");
            PlayerCoverageSystem.s_coverageKeys = new Il2CppStructArray<int>(PluginConfig.MaxPlayers);
        }
    }

    [HarmonyPatch(nameof(PlayerCoverageSystem.GetCoverageKey), [typeof(int)])]
    [HarmonyPrefix]
    public static bool GetCoverageKey__Prefix(ref int __result, int activeID)
    {
        L.LogExecutingMethod();

        if (activeID < 0)
        {
            // I doubt this will change, but let the original function handle it.
            return HarmonyControlFlow.Execute;
        }

        if (activeID >= PlayerCoverageSystem.s_coverageKeys.Length)
        {
            L.Error($"{nameof(activeID)} was greater than the size of s_coverageKeys ({PlayerCoverageSystem.s_coverageKeys.Length}).");

            // Again let the original function handle this: we at least have logged it to know.
            return HarmonyControlFlow.Execute;
        }

        __result = PlayerCoverageSystem.s_coverageKeys[activeID];
        return HarmonyControlFlow.DontExecute;
    }
}

//
// ! WARNING WARNING WARNING !
// TODO: This causes m_playerCoverage to be set to null? why?
//       Converting to manual patch for now.
//
// [HarmonyPatch(typeof(PlayerCoverageSystem.PlayerCoverageDataSet_Node))]
//
// public static class PlayerCoverageDataSet_NodePatch
// {
//     [HarmonyPatch(MethodType.Constructor)]
//     [HarmonyPostfix]
//
//     public static void Constructor__Postfix(PlayerCoverageSystem.PlayerCoverageDataSet_Node? __instance)
//     {
//         L.Verbose($"{nameof(PlayerCoverageDataSet_NodePatch)}::{nameof(Constructor__Postfix)}");
//         L.Assert(__instance != null, "instance was null");
//
//         if (__instance!.m_coverageDatas.Length < PluginConfig.MaxPlayers)
//         {
//             L.Verbose($"Expanding m_coverageDatas size from {__instance.m_coverageDatas.Length} to {PluginConfig.MaxPlayers}.");
//             __instance.m_coverageDatas = new Il2CppReferenceArray<PlayerCoverageSystem.PlayerCoverageData>(PluginConfig.MaxPlayers);
//
//             for (var i = 0; i < __instance.m_coverageDatas.Length; i++)
//             {
//                 L.Verbose($"Instantiating PlayerCoverageData at index #{i} in m_coverageDatas.");
//                 __instance.m_coverageDatas[i] = new PlayerCoverageSystem.PlayerCoverageData();
//             }
//         }
// #if DEBUG
//         else
//         {
//             L.DebugWarning($"m_coverageDatas size was >= {nameof(PluginConfig.MaxPlayers)}?");
//         }
// #endif
//     }
// }

// TODO: I have not actually seen SetupData get called in log yet... Is this working?
[HarmonyPatch(typeof(PlayerCoverageSystem.PlayerCoverageDataSet_Portal))]

public static class PlayerCoverageDataSet_PortalPatch
{
    [HarmonyPatch(nameof(PlayerCoverageSystem.PlayerCoverageDataSet_Portal.SetupData))]
    [HarmonyPostfix]

    public static void SetupData__Postfix(PlayerCoverageSystem.PlayerCoverageDataSet_Portal? __instance)
    {
        L.Verbose($"{nameof(PlayerCoverageDataSet_PortalPatch)}::{nameof(SetupData__Postfix)}");
        L.Assert(__instance != null, "instance was null");
        __instance!.m_coverageDatas = new Il2CppReferenceArray<PlayerCoverageSystem.PlayerCoverageData>(PluginConfig.MaxPlayers);

        for (var i = 0; i < __instance.m_coverageDatas.Length; i++)
        {
            L.Verbose($"Instantiating PlayerCoverageData at index #{i} in m_coverageDatas.");
            __instance.m_coverageDatas[i] = new PlayerCoverageSystem.PlayerCoverageData();
        }
    }
}
