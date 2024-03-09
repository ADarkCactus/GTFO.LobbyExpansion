using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Player;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(PlayerVoiceManager))]
public static class PlayerVoiceManagerPatch
{
    [HarmonyPatch(nameof(PlayerVoiceManager.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(PlayerVoiceManager __instance)
    {
        L.Verbose("PlayerVoiceManager::Setup postfix");

        if (__instance.m_playerVoices.Length < PluginConfig.MaxPlayers)
        {
            L.Verbose("Adjusting m_playerVoices size.");
            __instance.m_playerVoices = new Il2CppReferenceArray<PlayerVoice>(PluginConfig.MaxPlayers);
        }
    }
}
