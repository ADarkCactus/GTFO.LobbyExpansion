using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SNetwork;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(SNet_PlayerSlotManager))]
public static class SNet_PlayerSlotManagerPatch
{
    [HarmonyPatch(nameof(SNet_PlayerSlotManager.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(SNet_PlayerSlotManager __instance)
    {
        L.LogExecutingMethod();
        __instance.PlayerSlots = new Il2CppReferenceArray<SNet_Slot>(PluginConfig.MaxPlayers);
        __instance.CharacterSlots = new Il2CppReferenceArray<SNet_Slot>(PluginConfig.MaxPlayers);
        __instance.m_playerSlotPermissions = new Il2CppStructArray<SNet_PlayerSlotManager.SlotPermission>(PluginConfig.MaxPlayers);

        for (var i = 0; i < PluginConfig.MaxPlayers; i++)
        {
            __instance.PlayerSlots[i] = new SNet_Slot(i);
            __instance.CharacterSlots[i] = new SNet_Slot(i);
            __instance.InternalSetSlotPermission(i, SNet_PlayerSlotManager.SlotPermission.None);

            if (i > 3)
                __instance.m_playerSlotPermissions[i] = PluginConfig.GetExtraSlotPermission(i);
        }
    }
}
