using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Player;
using UnityEngine;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(PlayerManager))]
public static class PlayerManagerPatch
{
    [HarmonyPatch(nameof(PlayerManager.GetSpawnPoint))]
    [HarmonyPrefix]
    public static bool GetSpawnPoint__Prefix(PlayerspawnpointType type, ref int referenceID)
    {
        L.Verbose($"{nameof(PlayerManagerPatch)}::{nameof(GetSpawnPoint__Prefix)} ({nameof(type)}: {type}, {nameof(referenceID)}: {referenceID})");

        // Avoid limit by re-using the same spawn-points.
        if (referenceID > 3)
        {
            var newReferenceId = referenceID % 4;
            L.Verbose($"{nameof(GetSpawnPoint__Prefix)} changed referenceID from {referenceID} to {newReferenceId}.");
            referenceID = newReferenceId;
        }

        return HarmonyControlFlow.Execute;
    }

    [HarmonyPatch(nameof(PlayerManager.Awake))]
    [HarmonyPostfix]
    public static void Awake__Postfix(PlayerManager? __instance)
    {
        L.Verbose($"{nameof(PlayerManagerPatch)}::{nameof(Awake__Postfix)}");
        L.Assert(__instance != null, "__instance was null");
        L.Assert(
            __instance!.BotAIData.PositionReservations.Length == PluginConfig.MaxPlayers,
            $"{nameof(__instance.BotAIData.PositionReservations)} was {__instance.BotAIData.PositionReservations.Length} when the manual patch should have set it to {PluginConfig.MaxPlayers}.");
        L.Assert(
            __instance!.BotAIData.ObjectReservations.Length == PluginConfig.MaxPlayers,
            $"{nameof(__instance.BotAIData.ObjectReservations)} was {__instance.BotAIData.ObjectReservations.Length} when the manual patch should have set it to {PluginConfig.MaxPlayers}.");

        for (var i = 4; i < PluginConfig.MaxPlayers; i++)
        {
            L.Verbose($"Initializing BotAIData position and object reservation at index #{i}.");
            __instance.BotAIData.PositionReservations[i] = new Il2CppSystem.Collections.Generic.List<PlayerManager.PositionReservation>(PluginConfig.MaxPlayers);
            __instance.BotAIData.ObjectReservations[i] = new Il2CppSystem.Collections.Generic.List<PlayerManager.ObjectReservation>(PluginConfig.MaxPlayers);
        }
    }

    [HarmonyPatch(nameof(PlayerManager.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(PlayerManager __instance)
    {
        L.Verbose($"{nameof(PlayerManagerPatch)}::{nameof(Setup__Postfix)}");
        L.Assert(__instance != null, "__instance was null");

        L.Verbose("Fixing available spawnpoints...");
        FixAvailableSpawnpoints(__instance!);

        L.Verbose("Fixing player colors...");
        FixPlayerColors(__instance!);
    }

    private static void FixPlayerColors(PlayerManager __instance)
    {
        var extraColors = new[]
        {
            ColorExt.Hex("FFA500"), // orange
            Color.yellow,
            Color.magenta,
            Color.white,
        };

        var original = __instance!.m_playerColors;
        __instance.m_playerColors = new Il2CppStructArray<Color>(PluginConfig.MaxPlayers);

        for (var i = 0; i < PluginConfig.MaxPlayers; i++)
        {
            var color = i < original.Length ? original[i] : extraColors[i % extraColors.Length];
            L.Verbose($"Setting new player color at index #{i} to {color}.");
            __instance.m_playerColors[i] = color;
        }

        L.Verbose($"m_playerColors now contains {__instance.m_playerColors.Count} colors.");
    }

    private static void FixAvailableSpawnpoints(PlayerManager playerManager)
    {
        const int expectedSpawnpointTypeCount = 5;
        var typeCount = Enum.GetValues(typeof(PlayerspawnpointType)).Length;

        if (typeCount != expectedSpawnpointTypeCount)
            L.Warning($"{nameof(PlayerspawnpointType)} contained {typeCount} types, but {expectedSpawnpointTypeCount} were expected. This may or may not work as expected!");

        for (var i = 0; i < typeCount; i++)
        {
            playerManager.m_availableSpawnpoints[i] = new Il2CppReferenceArray<PlayerSpawnpoint>(PluginConfig.MaxPlayers);
            L.Verbose($"Set available spawnpoint at index #{i} to new max player size {(long)PluginConfig.MaxPlayers}.");
        }
    }
}
