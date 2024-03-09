using BoosterImplants;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(BoosterImplantManager))]
public static class BoosterImplantManagerPatch
{
    [HarmonyPatch(nameof(BoosterImplantManager.Awake))]
    [HarmonyPostfix]
    public static void Awake__Postfix(BoosterImplantManager __instance)
    {
        L.LogExecutingMethod();

        if (__instance.m_boosterPlayers.Length >= PluginConfig.MaxPlayers)
            return;

        L.Verbose($"Expanding {nameof(__instance.m_boosterPlayers)} size.");
        __instance.m_boosterPlayers = new Il2CppReferenceArray<PlayerBoosterImplantState>(PluginConfig.MaxPlayers);
    }
}
