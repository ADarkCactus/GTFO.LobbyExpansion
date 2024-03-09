using HarmonyLib;
using SNetwork;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(SNet_SessionHub))]
public static class SNet_SessionHubPatch
{
    [HarmonyPatch(nameof(SNet_SessionHub.TryAddBotToSession))]
    [HarmonyPostfix]
    public static void TryAddBotToSession__Postfix(SNet_SessionHub __instance, ref bool __result, SNet_Player bot)
    {
        L.LogExecutingMethod($"{nameof(__result)}: {__result}");

        // We already added a bot, ignore.
        if (__result)
            return;

        if (!bot.IsBot || !SNet.IsMaster)
            return;

        if (__instance.PlayersInSession.Count > PluginConfig.MaxPlayers - 1 || __instance.PlayersInSession.Contains(bot))
        {
            L.Verbose("No room to add another bot.");
            return;
        }

        L.Verbose("Adding extra bot.");
        __instance.AddPlayerToSession(bot, true);
        __result = true;
    }
}
