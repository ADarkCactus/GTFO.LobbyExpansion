using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(DramaManager))]
public static class DramaManagerPatch
{
    [HarmonyPatch(nameof(DramaManager.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix()
    {
        L.LogExecutingMethod();
        ResetDramaFieldsProperly();
    }

    [HarmonyPatch(nameof(DramaManager.OnLevelCleanup))]
    [HarmonyPostfix]
    public static void OnLevelCleanup__Postfix()
    {
        L.LogExecutingMethod();
        ResetDramaFieldsProperly();
    }

    private static void ResetDramaFieldsProperly()
    {
        DramaManager.SyncedPlayerStates = new Il2CppStructArray<DRAMA_State>(PluginConfig.MaxPlayers);
        DramaManager.SyncedPlayerTensions = new Il2CppStructArray<float>(PluginConfig.MaxPlayers);

        for (var i = 0; i < DramaManager.SyncedPlayerStates.Length; i++)
            DramaManager.SyncedPlayerStates[i] = DRAMA_State.ElevatorIdle;
    }
}
