using CellMenu;
using HarmonyLib;
using SNetwork;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CM_PageLoadout.__c__DisplayClass38_1))]
public class CM_PageLoadoutDisplayClassPatch
{
    [HarmonyPatch(nameof(CM_PageLoadout.__c__DisplayClass38_1._ShowPermissionWindow_b__1))]
    public static void Prefix(CM_PageLoadout.__c__DisplayClass38_1 __instance)
    {
        var num = 0;
        for (var i = 0; i < __instance.field_Public___c__DisplayClass38_0_0.playerIndex; i++)
        {
            var player = SNet.Slots.PlayerSlots[i].player;
            if (player == null || !player.IsLocal)
            {
                num++;
            }
        }

        if (num < 3)
        {
            CellSettingsManager.SettingsData.Player.LobbySlotPermissions[num] = __instance.permission;
            return;
        }

        PluginConfig.SetExtraLobbySlotPermissions(num, __instance.permission);
    }
}
