using BepInEx.Unity.IL2CPP.Utils.Collections;
using CellMenu;
using GTFO.LobbyExpansion.Util;
using HarmonyLib;
using SNetwork;
using UnityEngine;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CM_PageExpeditionSuccess))]
public static class CM_PageExpeditionSuccessPatch
{
    private static bool _needPositionUpdate;
    private static DateTime _lastVisibilityUpdate = DateTime.Now;

    // [HarmonyPatch(nameof(CM_PageExpeditionSuccess.OnEnable))]
    // [HarmonyPrefix]
    // public static bool OnEnable__Prefix(CM_PageExpeditionSuccess __instance)
    // {
    //     return HarmonyControlFlow.Execute;
    // }

    [HarmonyPatch(nameof(CM_PageExpeditionSuccess.OnEnable))]
    [HarmonyPostfix]
    public static void OnEnable__Postfix(CM_PageExpeditionSuccess __instance)
    {
        L.LogExecutingMethod();
        _pagination.PageIndex = 0;
        UpdateCustomButtons();
        _needPositionUpdate = true;
        UpdateVisiblePlayerReports(__instance);
    }

    [HarmonyPatch(nameof(CM_PageExpeditionSuccess.Update))]
    [HarmonyPostfix]
    public static void Update__Postfix(CM_PageExpeditionSuccess __instance)
    {
        if (_needPositionUpdate)
        {
            L.Verbose("Updating position of extra player reports.");

            for (var i = 4; i < __instance.m_playerReports.Length; i++)
            {
                var transform = __instance.m_playerReports[i % 4].transform;
                __instance.m_playerReports[i].transform.position = transform.position;

                // Ensure extended slots have the appropriate parent as well (if (i < this.m_playerReportAligns.Length) will cause them not to)
                __instance.m_playerReports[i].transform.parent = transform.parent;
            }

            _needPositionUpdate = false;
        }

        // This is ugly, but oh well. The minor amount of flickering is fine.
        if (DateTime.Now > _lastVisibilityUpdate)
        {
            _lastVisibilityUpdate = DateTime.Now + TimeSpan.FromMilliseconds(50);
            UpdateVisiblePlayerReports(__instance);
        }

        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            _pagination.PageDown();
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            _pagination.PageUp();
        }
    }

    private static void UpdateVisiblePlayerReports(CM_PageExpeditionSuccess page)
    {
        if (page.m_playerReports == null)
            return;

        // Only show the lobby bars that we're currently viewing
        var playerSlots = SNet.Slots.PlayerSlots;
        var minVisibleIndex = _pagination.PageIndex * 4;
        var maxVisibleIndex = Math.Min(minVisibleIndex + 4, PluginConfig.MaxPlayers);

        for (var i = 0; i < page.m_playerReports.Count; i++)
        {
            if (page.m_playerReports[i] == null)
                continue;

            var visible = true;

            if (i < minVisibleIndex)
            {
                //L.Verbose($"Hiding lobby bar at slot {i} since it is below the minimum visible index {minVisibleIndex}.");
                visible = false;
            }
            else if (i >= maxVisibleIndex)
            {
                //L.Verbose($"Hiding lobby bar at slot {i} since it is above the maximum visible index {maxVisibleIndex}.");
                visible = false;
            }
            else if (i >= playerSlots.Count || playerSlots[i].player == null)
            {
                //L.Verbose($"Hiding extra slot {i} since we don't have a player in that slot.");
                visible = false;
            }

            page.m_playerReports[i].gameObject.SetActive(visible);
        }
    }

    private static readonly Pagination _pagination = new();

    private static void OnPageChanged()
    {
        L.Verbose("Updating visible player reports.");
        UpdateVisiblePlayerReports(MainMenuGuiLayer.Current.PageExpeditionSuccess);
        UpdateCustomButtons();
    }

    private static CM_Item _buttonPageUp = null!;
    private static CM_Item _buttonPageDown = null!;

    private static void UpdateCustomButtons()
    {
        var canPageDown = _pagination.CanPageDown();
        var canPageUp = _pagination.CanPageUp();

        _buttonPageDown.SetCoolButtonEnabled(canPageDown);
        _buttonPageUp.SetCoolButtonEnabled(canPageUp);
    }

    [HarmonyPatch(nameof(CM_PageExpeditionSuccess.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(CM_PageExpeditionSuccess __instance, MainMenuGuiLayer guiLayer)
    {
        _pagination.OnPageChanged += OnPageChanged;

        CoroutineManager.StartCoroutine(CoroutineHelpers.NextFrame(() =>
        {
            _buttonPageUp = CoolButton.InstantiateSquareButton(__instance.m_btnLeaveExpedition.transform,
                new Vector3(320, 42, 0), hideText: true, displayArrow: true);

            _buttonPageDown = CoolButton.InstantiateSquareButton(__instance.m_btnLeaveExpedition.transform,
                new Vector3(-320, 42, 0), hideText: true, displayArrow: true, flipArrow: true);

            UpdateCustomButtons();

            _buttonPageUp.OnBtnPressCallback = new Action<int>(_ =>
            {
                _pagination.PageUp();
            });

            _buttonPageDown.OnBtnPressCallback = new Action<int>(_ =>
            {
                _pagination.PageDown();
            });

        }).WrapToIl2Cpp());
    }


}
