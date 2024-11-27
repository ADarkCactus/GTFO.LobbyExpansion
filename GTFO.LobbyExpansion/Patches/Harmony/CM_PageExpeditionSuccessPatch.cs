using CellMenu;
using HarmonyLib;
using SNetwork;
using UnityEngine;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(CM_PageExpeditionSuccess))]
public static class CM_PageExpeditionSuccessPatch
{
    private static bool _pageDownDebounce;
    private static bool _pageUpDebounce;
    private static bool _needPositionUpdate;
    private static DateTime _lastVisibilityUpdate = DateTime.Now;
    public static int PageIndex { get; set; }

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
        PageIndex = 0;
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

        var updateVisibility = false;
        var maxPageIndex = (int)Math.Ceiling(PluginConfig.MaxPlayers / 4.0d) - 1;

        if (PageIndex < maxPageIndex)
        {
            if (!_pageDownDebounce && Input.GetKeyDown(KeyCode.PageDown))
            {
                L.Verbose($"Showing next set of players {PageIndex}.");
                PageIndex += 1;
                _pageDownDebounce = true;
                updateVisibility = true;
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
                updateVisibility = true;
            }
        }

        if (_pageUpDebounce && Input.GetKeyUp(KeyCode.PageUp))
        {
            L.Verbose("Debounce off for page up.");
            _pageUpDebounce = false;
        }

        if (updateVisibility)
        {
            L.Verbose("Updating visible player reports.");
            UpdateVisiblePlayerReports(__instance);
        }

        if (GameStateManager.Current.m_nextState != eGameStateName.ExpeditionSuccess)
        {
            switchbutton.SetVisible(false);
        }
        else switchbutton.SetVisible(true);
    }

    private static void UpdateVisiblePlayerReports(CM_PageExpeditionSuccess page)
    {
        if (page.m_playerReports == null)
            return;

        // Only show the lobby bars that we're currently viewing
        var playerSlots = SNet.Slots.PlayerSlots;
        var minVisibleIndex = PageIndex * 4;
        var maxVisibleIndex = Math.Min(minVisibleIndex + 4, PluginConfig.MaxPlayers);

        for (var i = 0; i < page.m_playerReports.Count; i++)
        {
            if (page.m_playerReports[i] == null)
                continue;

            var visible = true;

            if (i < minVisibleIndex)
            {
                L.Verbose($"Hiding lobby bar at slot {i} since it is below the minimum visible index {minVisibleIndex}.");
                visible = false;
            }
            else if (i >= maxVisibleIndex)
            {
                L.Verbose($"Hiding lobby bar at slot {i} since it is above the maximum visible index {maxVisibleIndex}.");
                visible = false;
            }
            else if (i >= playerSlots.Count || playerSlots[i].player == null)
            {
                L.Verbose($"Hiding extra slot {i} since we don't have a player in that slot.");
                visible = false;
            }

            page.m_playerReports[i].gameObject.SetActive(visible);
        }
    }

    public static string buttonLabel = "SWITCH LOBBIES";
    public static Vector3 buttonPosition = new(350, -540, 50);
    public static Vector3 buttonScale = new(0.5f, 0.5f, 0);
    public static CM_Item switchbutton;

    [HarmonyPatch(nameof(CM_PageExpeditionSuccess.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(CM_PageExpeditionSuccess __instance, MainMenuGuiLayer guiLayer)
    {
        // Why does this class use GuiAnchor.TopLeft and the other uses GuiAnchor.TopCenter?
        var switchButton = __instance.m_guiLayer.AddRectComp(guiLayer.PageLoadout.m_readyButtonPrefab, GuiAnchor.TopLeft,
            new Vector2(200f, 20f), __instance.m_btnLeaveExpedition.transform).TryCast<CM_Item>();
        switchButton.SetText(buttonLabel);
        switchButton.gameObject.transform.position = buttonPosition;
        switchButton.gameObject.SetActive(true);
        switchButton.SetVisible(true);

        Action<int> value = delegate (int id)
        {
            var maxPageIndex = (int)Math.Ceiling(PluginConfig.MaxPlayers / 4.0d) - 1;
            if (PageIndex == 0)
            {
                PageIndex += 1;
                UpdateVisiblePlayerReports(__instance);
            }
            else if (PageIndex == 1)
            {
                PageIndex -= 1;
                UpdateVisiblePlayerReports(__instance);
            }
        };
        switchButton.OnBtnPressCallback += value;

        switchbutton = switchButton;
    }
}
