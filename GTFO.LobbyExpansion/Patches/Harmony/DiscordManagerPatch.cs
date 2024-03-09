using HarmonyLib;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(DiscordManager))]
public class DiscordManagerPatch
{
    [HarmonyPatch(nameof(DiscordManager.GetCharacterNickname))]
    [HarmonyPostfix]
    public static void GetCharacterNickname__Postfix(ref string __result, int characterIndex)
    {
        L.LogExecutingMethod($"{nameof(characterIndex)}: {characterIndex}, {nameof(__result)}: {__result}");

        if (characterIndex <= 3)
            return;

        __result = PluginConfig.GetExtraSlotNickname(characterIndex);
        L.Verbose($"Corrected character nickname for index #{characterIndex} to {__result}.");
    }
}
