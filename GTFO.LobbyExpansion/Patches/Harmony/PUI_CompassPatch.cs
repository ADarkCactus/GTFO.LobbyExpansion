using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(PUI_Compass))]
public static class PUI_CompassPatch
{
    [HarmonyPatch(nameof(PUI_Compass.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(PUI_Compass __instance)
    {
        L.LogExecutingMethod();
        __instance!.m_playerPingMarkers = new Il2CppReferenceArray<NavMarker>(PluginConfig.MaxPlayers);
        __instance.m_playerNameMarkers = new Il2CppReferenceArray<NavMarker>(PluginConfig.MaxPlayers);
        __instance.m_playerMarkersWorldPos = new Il2CppStructArray<Vector3>(PluginConfig.MaxPlayers);
        __instance.m_playerPingMarkersActive = new Il2CppStructArray<bool>(PluginConfig.MaxPlayers);
        __instance.m_playerNameMarkersVisible = new Il2CppStructArray<bool>(PluginConfig.MaxPlayers);

        for (var i = 0; i < PluginConfig.MaxPlayers; i++)
        {
            L.Verbose($"Setting up compass stuff at index #{i}.");
            __instance.m_playerPingMarkers[i] = __instance.SpawnCompassObj(__instance.m_playerMarkerPrefab, 0f, 0f, false).GetComponent<NavMarker>();
            __instance.m_playerPingMarkers[i].transform.localEulerAngles = new Vector3(0f, 0f, 180f);
            __instance.m_playerPingMarkers[i].transform.localScale = new Vector3(__instance.m_navMarkerScale, __instance.m_navMarkerScale, __instance.m_navMarkerScale);
            __instance.m_playerPingMarkers[i].SetVisible(false);
            __instance.m_playerPingMarkers[i].SetPinEnabled(true);
            __instance.m_playerNameMarkers[i] = __instance.SpawnCompassObj(__instance.m_playerMarkerPrefab, 0f, 0f, false).GetComponent<NavMarker>();
            __instance.m_playerNameMarkers[i].transform.localScale = new Vector3(__instance.m_navMarkerScale, __instance.m_navMarkerScale, __instance.m_navMarkerScale);
            __instance.m_playerNameMarkers[i].SetStyle(eNavMarkerStyle.PlayerInCompass);
            __instance.m_playerNameMarkers[i].SetState(NavMarkerState.Visible);
            __instance.m_playerNameMarkers[i].SetVisible(false);
            __instance.m_playerNameMarkers[i].SetPinEnabled(false);
            __instance.m_playerPingMarkersActive[i] = false;
        }
    }
}
