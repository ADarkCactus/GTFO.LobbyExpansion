using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using CellMenu;
using GTFO.LobbyExpansion.Util;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SNetwork;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CM_PageLoadout))]
public static class CM_PageLoadoutPatch
{
    [HarmonyPatch(nameof(CM_PageLoadout.Setup))]
    [HarmonyPrefix]
    public static bool Setup__Prefix(CM_PageLoadout __instance, MainMenuGuiLayer guiLayer)
    {
        L.LogExecutingMethod();
        var original = __instance.m_playerInfoHolders;
        __instance.m_playerInfoHolders = new Il2CppReferenceArray<Transform>(PluginConfig.MaxPlayers);

        // i < 4 to skip the dupe 5th slot
        for (var i = 0; i < 4; i++)
            __instance.m_playerInfoHolders[i] = original[i];

        // TODO: use __instance.gameObject and find through that
        var playerPillarsGameObj = GameObject.Find("GUI/CellUI_Camera(Clone)/MainMenuLayer/CM_PageLoadout_CellUI(Clone)/PlayerMovement/PlayerPillars");
        L.Assert(playerPillarsGameObj != null, $"{nameof(playerPillarsGameObj)} was null!");

        for (var i = 4; i < PluginConfig.MaxPlayers; i++)
        {
            var rootName = $"Player{i + 1}Root";
            L.Verbose($"Creating extra player root ({rootName}).");

            // Create new PlayerRoot game object (nothing special)
            var newRoot = new GameObject(rootName);

            // Copy positions from base pillar positions
            var originalPillarAtIndex = __instance.m_playerInfoHolders[i % 4];
            newRoot.transform.position = originalPillarAtIndex.position;
            newRoot.transform.localPosition = originalPillarAtIndex.localPosition;
            newRoot.transform.localScale = originalPillarAtIndex.localScale;
            newRoot.transform.SetParent(playerPillarsGameObj!.transform);

            // TODO: set UI layer properly. idk if this is how to do it.
            newRoot.layer = LayerMask.NameToLayer("UI");

            newRoot.SetActive(true);

            __instance.m_playerInfoHolders[i] = newRoot.transform;
        }

        return HarmonyControlFlow.Execute;
    }

    [HarmonyPatch(nameof(CM_PageLoadout.ArrangePlayerPillarSpacing))]
    [HarmonyPostfix]
    public static void ArrangePlayerPillarSpacing__Postfix(CM_PageLoadout __instance)
    {
        //L.LogExecutingMethod();
        var originalPositions = new[]
        {
            __instance.m_playerInfoHolders[0].transform.position,
            __instance.m_playerInfoHolders[1].transform.position,
            __instance.m_playerInfoHolders[2].transform.position,
            __instance.m_playerInfoHolders[3].transform.position
        };

        for (var i = 4; i < PluginConfig.MaxPlayers; i++)
        {
            var rootName = $"Player{i + 1}Root";
            //L.Verbose($"Updating extra player root position ({rootName}).");
            var originalPillarAtIndex = __instance!.m_playerInfoHolders[i % 4];
            __instance.m_playerInfoHolders[i].transform.position = originalPillarAtIndex.position;
            __instance.m_playerInfoHolders[i].transform.localPosition = originalPillarAtIndex.localPosition;
            __instance.m_playerInfoHolders[i].transform.localScale = originalPillarAtIndex.localScale;
        }

        // Only show the lobby bars that we're currently viewing
        var minVisibleIndex = _pagination.PageIndex * 4;
        var maxVisibleIndex = Math.Min(minVisibleIndex + 4, PluginConfig.MaxPlayers);

        for (var i = 0; i < __instance.m_playerInfoHolders.Count; i++)
        {
            // NOTE: For hiding the inactive lobby bars:
            // - Don't use localScale otherwise the console will spam with look viewing rotation vector is zero AND messes up the UI elements
            // - Don't set active to false otherwise the lobby bar will not update until it's back on screen again

            if (i < minVisibleIndex)
            {
                //L.Verbose($"Hiding lobby bar at slot {i} since it is below the minimum visible index {minVisibleIndex}.");
                __instance.m_playerInfoHolders[i].gameObject.transform.position = new Vector3(5000, 5000, 5000);
                continue;
            }

            if (i >= maxVisibleIndex)
            {
                //L.Verbose($"Hiding lobby bar at slot {i} since it is above the maximum visible index {maxVisibleIndex}.");
                __instance.m_playerInfoHolders[i].gameObject.transform.position = new Vector3(5000, 5000, 5000);
                continue;
            }

            __instance.m_playerInfoHolders[i].transform.position = originalPositions[i % originalPositions.Length];
        }
    }

    public static void UpdatePlayerPillars()
    {
        CM_PageLoadout.Current.ArrangePlayerPillarSpacing();
    }

    [HarmonyPatch(nameof(CM_PageLoadout.Update))]
    [HarmonyPostfix]
    public static void Update__Postfix(CM_PageLoadout __instance)
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (_pagination.PageDown())
                UpdateCustomButtons();
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            if (_pagination.PageUp())
                UpdateCustomButtons();
        }
    }

    [HarmonyPatch(nameof(CM_PageLoadout.ApplyPlayerSlotPermissionsFromSettings))]
    [HarmonyPostfix]
    public static void ApplyPlayerSlotPermissionsFromSettings__Postfix(CM_PageLoadout __instance)
    {
        L.LogExecutingMethod();
        // TODO: This may call the handlers twice, but does it really matter?
        var flag = false;

        for (var i = 4; i < PluginConfig.MaxPlayers; i++)
        {
            var permission = PluginConfig.GetExtraSlotPermission(i);

            if (SNet.Slots.PlayerSlots[i].player == null || !SNet.Slots.PlayerSlots[i].player.IsLocal)
                flag = SNet.Slots.SetSlotPermission(i, permission) || flag;
            else if (SNet.Slots.PlayerSlots[i].player.IsLocal)
                flag = SNet.Slots.SetSlotPermission(i, SNet_PlayerSlotManager.SlotPermission.Human) || flag;
        }

        if (!flag)
        {
            L.Verbose("Calling OnSlotsStatusChange.");
            SNet.Core.OnSlotsStatusChanged();
        }

        // This was added after R6 mono
        if (SNet_Events.OnLobbyPermissionsSet != null)
        {
            L.Verbose("Calling OnLobbyPermissionsSet.");
            SNet_Events.OnLobbyPermissionsSet.Invoke();
        }
    }

    [HarmonyPatch(nameof(CM_PageLoadout.OnDisable))]
    [HarmonyPostfix]
    public static void OnDisable__Postfix()
    {
        L.LogExecutingMethod();
        //PageIndex = 0;
    }

    private static CM_Item _centerButton = null!;
    private static CM_Item _buttonPageDown = null!;
    private static CM_Item _buttonPageUp = null!;

    private static void UpdateCustomButtons()
    {
        var canPageDown = _pagination.CanPageDown();
        var canPageUp = _pagination.CanPageUp();

        _buttonPageDown.SetCoolButtonEnabled(canPageDown);
        _buttonPageUp.SetCoolButtonEnabled(canPageUp);

        _centerButton.SetText($"<color=white>P{_pagination.PageIndex + 1}</color>");
    }

    private const string DECOR_TEXT = "//: Lobby Page Switcher";

    private static readonly Pagination _pagination = new();

    private static void OnPageChanged()
    {
        UpdatePlayerPillars();
    }

    [HarmonyPatch(nameof(CM_PageLoadout.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(CM_PageLoadout __instance, MainMenuGuiLayer guiLayer)
    {
        if (CM_PageLoadout.Current == null)
            return;

        _pagination.OnPageChanged += OnPageChanged;

        CoolButton.Setup(__instance);

        // You might ask yourself, "Why do we wait here for a frame instead of doing things immediately?"
        // Well, you see, *IL2CPP moment* :sunglasses:
        // (Can't instantiate a freshly created prefab, else destroyed things persist and values aren't set properly)
        CoroutineManager.StartCoroutine(CoroutineHelpers.NextFrame(() =>
        {
            var switcherHolder = new GameObject("LobbySwitcherHolder");

            var switcherTrans = switcherHolder.transform;
            switcherTrans.SetParent(__instance.m_readyButtonAlign.parent);
            switcherTrans.localPosition = __instance.m_readyButtonAlign.localPosition + new Vector3(0, 1150, 0);
            switcherTrans.localScale = Vector3.one * 0.9f;

            var decor = __instance.m_readyButtonAlign.Find("DecorText");
            decor = Object.Instantiate(decor, switcherTrans);
            decor.transform.localPosition = new Vector3(100, 0, 0);
            decor.transform.localScale = Vector3.one;
            decor.gameObject.SetActive(true);
            decor.GetComponent<TextMeshPro>().SetText(DECOR_TEXT);

            _centerButton = CoolButton.InstantiateSquareButton(switcherTrans, Vector3.zero);

            _buttonPageDown = CoolButton.InstantiateSquareButton(switcherTrans, new Vector3(-110, 0, 0), displayArrow: true, hideText: true, flipArrow: true);

            _buttonPageUp = CoolButton.InstantiateSquareButton(switcherTrans, new Vector3(110, 0, 0), displayArrow: true, hideText: true, flipArrow: false);

            UpdateCustomButtons();

            _centerButton.OnBtnPressCallback = new Action<int>(_ =>
            {
                var localPlayerPageIndex = Pagination.GetLocalPlayerPageIndexFromPillar();

                if (_pagination.PageIndex != localPlayerPageIndex)
                {
                    _pagination.PageIndex = localPlayerPageIndex;
                    UpdatePlayerPillars();
                }

                UpdateCustomButtons();
            });

            _buttonPageDown.OnBtnPressCallback = new Action<int>(_ =>
            {
                _pagination.PageDown();
                UpdateCustomButtons();
            });

            _buttonPageUp.OnBtnPressCallback = new Action<int>(_ =>
            {
                _pagination.PageUp();
                UpdateCustomButtons();
            });

        }).WrapToIl2Cpp());
    }
}
