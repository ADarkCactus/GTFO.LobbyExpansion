using HarmonyLib;
using Player;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(PlayerDialogManager))]
public static class PlayerDialogManagerPatch
{
    [HarmonyPatch(nameof(PlayerDialogManager.WantToStartDialog))]
    [HarmonyPatch([typeof(uint), typeof(PlayerAgent)])]
    [HarmonyPrefix]
    public static bool WantToStartDialog__Prefix(uint dialogID, PlayerAgent source)
    {
        L.LogExecutingMethod();

        if (source == null)
        {
            L.Verbose("Source was null.");
            return false;
        }

        return true;
    }
}
