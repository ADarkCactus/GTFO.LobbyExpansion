using CellMenu;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SNetwork;
using UnityEngine;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CM_PageLoadout))]
public static class CM_PageLoadoutPatch
{
    public static int PageIndex { get; set; }

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
        L.LogExecutingMethod();
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
            L.Verbose($"Updating extra player root position ({rootName}).");
            var originalPillarAtIndex = __instance!.m_playerInfoHolders[i % 4];
            __instance.m_playerInfoHolders[i].transform.position = originalPillarAtIndex.position;
            __instance.m_playerInfoHolders[i].transform.localPosition = originalPillarAtIndex.localPosition;
            __instance.m_playerInfoHolders[i].transform.localScale = originalPillarAtIndex.localScale;
        }

        // Only show the lobby bars that we're currently viewing
        var minVisibleIndex = PageIndex * 4;
        var maxVisibleIndex = Math.Min(minVisibleIndex + 4, PluginConfig.MaxPlayers);

        for (var i = 0; i < __instance.m_playerInfoHolders.Count; i++)
        {
            // NOTE: For hiding the inactive lobby bars:
            // - Don't use localScale otherwise the console will spam with look viewing rotation vector is zero AND messes up the UI elements
            // - Don't set active to false otherwise the lobby bar will not update until it's back on screen again

            if (i < minVisibleIndex)
            {
                L.Verbose($"Hiding lobby bar at slot {i} since it is below the minimum visible index {minVisibleIndex}.");
                __instance.m_playerInfoHolders[i].gameObject.transform.position = new Vector3(5000, 5000, 5000);
                continue;
            }

            if (i >= maxVisibleIndex)
            {
                L.Verbose($"Hiding lobby bar at slot {i} since it is above the maximum visible index {maxVisibleIndex}.");
                __instance.m_playerInfoHolders[i].gameObject.transform.position = new Vector3(5000, 5000, 5000);
                continue;
            }

            __instance.m_playerInfoHolders[i].transform.position = originalPositions[i % originalPositions.Length];
        }
    }

    private static bool _pageDownDebounce;
    private static bool _pageUpDebounce;

    [HarmonyPatch(nameof(CM_PageLoadout.Update))]
    [HarmonyPostfix]
    public static void Update__Postfix(CM_PageLoadout __instance)
    {
        var updateVisiblePillars = false;
        var maxPageIndex = (int)Math.Ceiling(PluginConfig.MaxPlayers / 4.0d) - 1;

        if (PageIndex < maxPageIndex)
        {
            if (!_pageDownDebounce && Input.GetKeyDown(KeyCode.PageDown))
            {
                L.Verbose($"Showing next set of players {PageIndex}.");
                PageIndex += 1;
                _pageDownDebounce = true;
                updateVisiblePillars = true;
            }
        }

        if (_pageDownDebounce && Input.GetKeyUp(KeyCode.PageDown))
        {
            L.Verbose("Debounce off for page down.");
            _pageDownDebounce = false;
        }

        if (PageIndex > 0)
        {
            if (!_pageUpDebounce && Input.GetKeyDown(KeyCode.PageUp))
            {
                L.Verbose($"Showing previous set of players {PageIndex}.");
                PageIndex -= 1;
                _pageUpDebounce = true;
                updateVisiblePillars = true;
            }
        }

        if (_pageUpDebounce && Input.GetKeyUp(KeyCode.PageUp))
        {
            L.Verbose("Debounce off for page up.");
            _pageUpDebounce = false;
        }

        if (updateVisiblePillars)
        {
            L.Verbose("Updating visible pillars.");
            __instance!.ArrangePlayerPillarSpacing();
        }

        if (!PlayfabMatchmakingManager.Current.IsMatchmakeInProgress)
        {
            bool isInLobby = SNet.IsInLobby;
            if (isInLobby)
            {
                if (!GameStateManager.IsReady)
                {
                    switchbutton.SetVisible(true);
                }
                else
                {
                    switchbutton.SetVisible(false);
                }
            }
        }

        if (GameStateManager.Current.m_nextState == eGameStateName.InLevel)
        {
            switchbutton.SetVisible(true);
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
        PageIndex = 0;
    }

    public static string buttonLabel = "SWITCH LOBBIES";
    public static Vector3 buttonPosition = new(450, -600, 50);
    public static Vector3 buttonScale = new(0.5f, 0.5f, 0);
    public static CM_Item switchbutton;

    [HarmonyPatch(nameof(CM_PageLoadout.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(CM_PageLoadout __instance, MainMenuGuiLayer guiLayer)
    {
        if (CM_PageLoadout.Current != null)
        {
            // Why does this class use GuiAnchor.TopCenter and the other uses GuiAnchor.TopLeft?
            var switchButton = __instance.m_guiLayer.AddRectComp(__instance.m_readyButtonPrefab, GuiAnchor.TopCenter,
                new Vector2(200f, 20f), __instance.m_readyButtonAlign).TryCast<CM_Item>();
            switchButton.SetText(buttonLabel);
            switchButton.gameObject.transform.position = buttonPosition;
            switchButton.gameObject.SetActive(true);
            switchButton.SetVisible(!GameStateManager.IsReady);

            Action<int> value = delegate (int id)
            {
                var maxPageIndex = (int)Math.Ceiling(PluginConfig.MaxPlayers / 4.0d) - 1;
                if (PageIndex == 0)
                {
                    PageIndex += 1;
                    __instance!.ArrangePlayerPillarSpacing();
                }
                else if (PageIndex == 1)
                {
                    PageIndex -= 1;
                    __instance!.ArrangePlayerPillarSpacing();
                }
            };
            switchButton.OnBtnPressCallback += value;

            switchbutton = switchButton;
        }
    }
}
