using HarmonyLib;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(DefaultCharacterLayouts))]
public class DefaultCharacterLayoutsPatch
{
    [HarmonyPatch(nameof(DefaultCharacterLayouts.GetDefaultVanityItems))]
    [HarmonyPrefix]
    public static bool GetDefaultVanityItems__Prefix(ref int characterIndex)
    {
        L.LogExecutingMethod($"{nameof(characterIndex)}: {characterIndex}");

        if (characterIndex > 3)
        {
            characterIndex %= 4;
            L.Verbose($"Clamping {nameof(characterIndex)} to {characterIndex}.");
        }

        return HarmonyControlFlow.Execute;
    }
}
