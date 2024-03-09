using CellMenu;
using HarmonyLib;
using SNetwork;
using UnityEngine;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CM_PageMap))]
public static class CM_PageMapPatch
{
    [HarmonyPatch(nameof(CM_PageMap.UpdatePlayerInventory))]
    [HarmonyPostfix]
    public static void UpdatePlayerInventory__Postfix(CM_PageMap __instance, SNet_Player player, int count)
    {
        // count 0 - 3 are "normal" players
        var inventoryUiItem = __instance.m_inventory[player.PlayerSlotIndex()];
        var originalPosition = inventoryUiItem.GetPosition();
        var newPosition = new Vector2(0f, 0f);
        var column = 0;

        for (var i = count; i > 3; i -= 4)
            column += 1;

        var spacingBetweenColumns = -__instance.m_inventoryOffsetPerPlayer * 1.3f;
        newPosition.x = originalPosition.x + (column * spacingBetweenColumns);

        // Same logic as the base game with count clamped.
        newPosition.y = -150f + (count % 4) * -__instance.m_inventoryOffsetPerPlayer;
        inventoryUiItem.SetPosition(newPosition);
    }
}
