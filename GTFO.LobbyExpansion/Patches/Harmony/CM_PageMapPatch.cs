using BepInEx.Unity.IL2CPP.Utils.Collections;
using CellMenu;
using GTFO.LobbyExpansion.Util;
using HarmonyLib;
using SNetwork;
using UnityEngine;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CM_PageMap))]
public static class CM_PageMapPatch
{
    private static readonly Pagination _pagination = new();

    private static CM_Item _buttonCenter = null!;
    private static CM_Item _buttonPageUp = null!;
    private static CM_Item _buttonPageDown = null!;

    private static void OnPageChanged()
    {
        UpdateCustomButtons();
        UpdateAllPlayerInventories();
    }

    private static void UpdateCustomButtons()
    {
        _buttonPageUp.SetCoolButtonEnabled(_pagination.CanPageUp());
        _buttonPageDown.SetCoolButtonEnabled(_pagination.CanPageDown());

        _buttonCenter.SetText($"<color=white>P{_pagination.PageIndex + 1}</color>");
    }

    [HarmonyPatch(nameof(CM_PageMap.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(CM_PageMap __instance)
    {
        _pagination.OnPageChanged += OnPageChanged;

        CoroutineManager.StartCoroutine(CoroutineHelpers.NextFrame(() =>
        {

            var pageButtonRoot = new GameObject("LobbyExpansion_PageButtonRoot");
            pageButtonRoot.transform.SetParent(__instance.m_btnGotoObjectives.transform);
            pageButtonRoot.transform.localPosition = new Vector3(40, -200, 0);
            pageButtonRoot.transform.localScale = Vector3.one * 0.6f;

            _buttonCenter = CoolButton.InstantiateSquareButton(pageButtonRoot.transform, Vector3.zero);

            _buttonPageDown = CoolButton.InstantiateSquareButton(pageButtonRoot.transform, new Vector3(-40, 80, 0), hideText: true, displayArrow: true);
            _buttonPageDown.transform.localRotation = Quaternion.Euler(0, 0, 90);

            _buttonPageUp = CoolButton.InstantiateSquareButton(pageButtonRoot.transform, new Vector3(-40, -160, 0), hideText: true, displayArrow: true, flipArrow: true);
            _buttonPageUp.transform.localRotation = Quaternion.Euler(0, 0, 90);

            UpdateCustomButtons();

            _buttonCenter.OnBtnPressCallback = new Action<int>(_ =>
            {
                var localPlayerPageIndex = Pagination.GetLocalPlayerPageFromSlotIndex();

                if (_pagination.PageIndex != localPlayerPageIndex)
                {
                    _pagination.PageIndex = localPlayerPageIndex;
                    UpdateAllPlayerInventories();
                }

                UpdateCustomButtons();
            });

            _buttonPageDown.OnBtnPressCallback = new Action<int>(_ =>
            {
                _pagination.PageDown();
            });

            _buttonPageUp.OnBtnPressCallback = new Action<int>(_ =>
            {
                _pagination.PageUp();
            });

        }).WrapToIl2Cpp());
    }

    private static void UpdateAllPlayerInventories()
    {
        var pageMap = CM_PageMap.Current;
        var num = 0;
        foreach (var player in SNet.Slots.SlottedPlayers)
        {
            if (!player.IsInSlot)
                continue;

            pageMap.UpdatePlayerInventory(player, num);
            num++;
        }
    }

    [HarmonyPatch(nameof(CM_PageMap.Update))]
    [HarmonyPostfix]
    public static void Update__Postfix()
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            _pagination.PageDown();
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            _pagination.PageUp();
        }
    }

    [HarmonyPatch(nameof(CM_PageMap.UpdatePlayerInventory))]
    [HarmonyPostfix]
    public static void UpdatePlayerInventory__Postfix(CM_PageMap __instance, SNet_Player player, int count)
    {
        // count 0 - 3 are "normal" players
        var inventoryUiItem = __instance.m_inventory[player.PlayerSlotIndex()];
        var originalPosition = inventoryUiItem.GetPosition();
        var newPosition = new Vector2(originalPosition.x, originalPosition.y);
        var column = 0;

        for (var i = count; i > 3; i -= 4)
            column += 1;

        if (_pagination.PageIndex != column)
        {
            newPosition.x = 5000;
        }

        // Same logic as the base game with count clamped.
        newPosition.y = -150f + (count % 4) * -__instance.m_inventoryOffsetPerPlayer;
        inventoryUiItem.SetPosition(newPosition);
    }
}
