using HarmonyLib;
using SNetwork;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(SNet_LobbyManager))]
public class SNet_LobbyManagerPatch
{
    [HarmonyPatch(nameof(SNet_LobbyManager.CreateLobby))]
    [HarmonyPrefix]
    public static bool CreateLobby__Prefix(SNet_LobbySettings settings, bool leaveSessionHub)
    {
        L.LogExecutingMethod();

        if (settings.m_playerLimit < PluginConfig.MaxPlayers)
        {
            settings.m_playerLimit = PluginConfig.MaxPlayers;
            L.Verbose($"Expanding m_playerLimit to {PluginConfig.MaxPlayers}.");
        }

        return true;
    }
}
