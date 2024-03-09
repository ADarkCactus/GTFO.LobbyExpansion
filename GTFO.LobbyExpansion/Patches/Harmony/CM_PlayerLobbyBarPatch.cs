using CellMenu;
using HarmonyLib;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CM_PlayerLobbyBar))]
public static class CM_PlayerLobbyBarPatch
{
    [HarmonyPatch(nameof(CM_PlayerLobbyBar.SpawnPlayerModel))]
    [HarmonyPrefix]
    public static bool SpawnPlayerModel__Prefix(CM_PlayerLobbyBar __instance, ref int index)
    {
        L.LogExecutingMethod($"{nameof(index)}: {index}");

        if (index > 3)
        {
            var clamped = index % 4;
            L.Verbose($"Clamping {nameof(index)} to {clamped}.");
            index = clamped;
        }

        return HarmonyControlFlow.Execute;
    }
}
