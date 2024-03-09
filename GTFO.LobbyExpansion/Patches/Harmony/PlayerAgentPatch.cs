using GameData;
using HarmonyLib;
using Player;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(PlayerAgent))]
public static class PlayerAgentPatch
{
    [HarmonyPatch(nameof(PlayerAgent.PlayerCharacterFilter), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool PlayerCharacterFilter__Getter__Prefix(PlayerAgent __instance, ref DialogCharFilter __result)
    {
        L.LogExecutingMethod();
        var charId = __instance.CharacterID;

        if (charId > 3)
        {
            var newCharId = charId % 4;
            __result = __instance.m_playerCharacters[newCharId];
            L.Verbose($"Clamping {nameof(__instance.CharacterID)} from {charId} to {newCharId}.");
            return HarmonyControlFlow.DontExecute;
        }

        return HarmonyControlFlow.Execute;
    }
}
