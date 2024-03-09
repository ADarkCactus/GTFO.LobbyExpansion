using HarmonyLib;
using Steamworks;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(SteamMatchmaking))]
public static class SteamMatchmakingPatch
{
    [HarmonyPatch(nameof(SteamMatchmaking.CreateLobby))]
    [HarmonyPrefix]
    public static bool CreateLobby__Prefix(ELobbyType eLobbyType, ref int cMaxMembers)
    {
        L.LogExecutingMethod($"{eLobbyType}, {cMaxMembers}");

        if (cMaxMembers < PluginConfig.MaxPlayers)
        {
            cMaxMembers = PluginConfig.MaxPlayers;
            L.Verbose($"Expanding CreateLobby cMaxMembers to {PluginConfig.MaxPlayers}.");
        }

        return true;
    }
}
