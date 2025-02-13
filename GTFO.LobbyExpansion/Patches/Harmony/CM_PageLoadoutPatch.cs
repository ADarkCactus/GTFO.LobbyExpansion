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
        var minVisibleIndex = PageIndex * 4;
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

    public static bool CanPageUp()
    {
        var maxPageIndex = (int)Math.Ceiling(PluginConfig.MaxPlayers / 4d) - 1;
        return PageIndex < maxPageIndex;
    }

    public static bool PageUp()
    {
        if (CanPageUp())
        {
            PageIndex++;
            UpdatePlayerPillars();
            return true;
        }

        return false;
    }

    public static bool CanPageDown()
    {
        return PageIndex > 0;
    }

    public static bool PageDown()
    {
        if (CanPageDown())
        {
            PageIndex--;
            UpdatePlayerPillars();
            return true;
        }

        return false;
    }

    public static int GetLocalPlayerPageIndex()
    {
        foreach (var lobbyBar in CM_PageLoadout.Current.m_playerLobbyBars)
        {
            if (!lobbyBar.m_player.IsLocal)
                continue;

            return GetPageFromSlotIndex(lobbyBar.PlayerSlotIndex);
        }

        return 0;
    }

    public static void UpdatePlayerPillars()
    {
        CM_PageLoadout.Current.ArrangePlayerPillarSpacing();
    }

    private static int GetPageFromSlotIndex(int slotIndex)
    {
        return (int) Math.Floor(slotIndex / 4f);
    }

    [HarmonyPatch(nameof(CM_PageLoadout.Update))]
    [HarmonyPostfix]
    public static void Update__Postfix(CM_PageLoadout __instance)
    {
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (PageDown())
                UpdateCustomButtons();
        }

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            if (PageUp())
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

    private const float ALPHA_OVER = 1f;
    private const float ALPHA_OVER_DISABLED = 0.25f;

    private const float ALPHA_OUT = 0.66f;
    private const float ALPHA_OUT_DISABLED = 0.25f;

    private static readonly Color COLOR_OUT = new Color(1, 1, 1, 0.66f);
    private static readonly Color COLOR_HOVER = new Color(1, 1, 1, 1f);
    private static readonly Color COLOR_DISABLED = new Color(1, 1, 1, 0.25f);

    private static void UpdateCustomButtons()
    {
        var canPageDown = CanPageDown();
        var canPageUp = CanPageUp();

        _buttonPageDown.m_spriteAlphaOut = canPageDown ? ALPHA_OUT : ALPHA_OUT_DISABLED;
        _buttonPageDown.m_spriteAlphaOver = canPageDown ? ALPHA_OVER : ALPHA_OVER_DISABLED;

        _buttonPageUp.m_spriteAlphaOut = canPageUp ? ALPHA_OUT : ALPHA_OUT_DISABLED;
        _buttonPageUp.m_spriteAlphaOver = canPageUp ? ALPHA_OVER : ALPHA_OVER_DISABLED;

        _buttonPageDown.m_spriteColorOut = canPageDown ? COLOR_OUT : COLOR_DISABLED;
        _buttonPageDown.m_spriteColorOver = canPageDown ? COLOR_HOVER : COLOR_DISABLED;

        _buttonPageUp.m_spriteColorOut = canPageUp ? COLOR_OUT : COLOR_DISABLED;
        _buttonPageUp.m_spriteColorOver = canPageUp ? COLOR_HOVER : COLOR_DISABLED;

        _buttonPageDown.OnHoverOut();
        _buttonPageUp.OnHoverOut();

        _buttonPageDown.m_collider.enabled = canPageDown;
        _buttonPageUp.m_collider.enabled = canPageUp;

        _centerButton.SetText($"<color=white>P{PageIndex + 1}</color>");
    }

    [HarmonyPatch(nameof(CM_PageLoadout.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix(CM_PageLoadout __instance, MainMenuGuiLayer guiLayer)
    {
        if (CM_PageLoadout.Current == null)
            return;

        if (_squareButtonPrefab == null)
            _squareButtonPrefab = CreateButtonPrefab(__instance);

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
            decor.GetComponent<TextMeshPro>().SetText(DecorText);

            _centerButton = InstantiateSquareButton(switcherTrans, Vector3.zero);

            _buttonPageDown = InstantiateSquareButton(switcherTrans, new Vector3(-110, 0, 0), displayArrow: true, hideText: true, flipArrow: true);

            _buttonPageUp = InstantiateSquareButton(switcherTrans, new Vector3(110, 0, 0), displayArrow: true, hideText: true, flipArrow: false);

            UpdateCustomButtons();

            _centerButton.OnBtnPressCallback = new Action<int>(_ =>
            {
                var localPlayerPageIndex = GetLocalPlayerPageIndex();

                if (PageIndex != localPlayerPageIndex)
                {
                    PageIndex = localPlayerPageIndex;
                    UpdatePlayerPillars();
                }

                UpdateCustomButtons();
            });

            _buttonPageDown.OnBtnPressCallback = new Action<int>(_ =>
            {
                PageDown();
                UpdateCustomButtons();
            });

            _buttonPageUp.OnBtnPressCallback = new Action<int>(_ =>
            {
                PageUp();
                UpdateCustomButtons();
            });

        }).WrapToIl2Cpp());
    }

    private static string DecorText { get; set; } = "//: Lobby Page Switcher";

    private static GameObject _squareButtonPrefab = null!;

    public static CM_Item InstantiateSquareButton(Transform parent, Vector3 position, bool hideText = false, bool displayArrow = false, bool flipArrow = false)
    {
        var button = Object.Instantiate(_squareButtonPrefab, parent);

        button.gameObject.SetActive(true);

        button.transform.localPosition = position;

        var cmButton = button.GetComponent<CM_Item>();

        if (hideText)
        {
            button.transform.Find("MainText").gameObject.SetActive(false);
        }

        var arrow = button.transform.Find("Arrow");

        arrow.gameObject.SetActive(displayArrow);

        if (flipArrow)
        {
            arrow.localRotation = Quaternion.Euler(0, 0, -90);
        }


        cmButton.m_hoverSpriteArray = button.gameObject.GetComponentsInChildren<SpriteRenderer>(includeInactive: true).ToArray();

        foreach (var renderer in cmButton.m_hoverSpriteArray)
        {
            renderer.color = new Color(1, 1, 1, 0.66f);
        }

        cmButton.m_spriteAlphaOver = 1f;
        cmButton.m_spriteAlphaOut = 0.66f;
        cmButton.m_alphaSpriteOnHover = true;

        cmButton.m_onBtnPress = new();

        cmButton.Setup();

        return cmButton;
    }

    private static GameObject CreateButtonPrefab(CM_PageLoadout __instance)
    {
        var button = Object.Instantiate(__instance.m_readyButtonPrefab).GetComponent<CM_Item>();
        var go = button.gameObject;
        var trans = go.transform;

        Object.Destroy(button); // Destroy CM_TimedButton

        button = go.AddComponent<CM_Item>();

        Object.Destroy(trans.Find("PressAndHold").gameObject);
        Object.Destroy(trans.Find("ProgressFillBase").gameObject);
        Object.Destroy(trans.Find("ProgressFill").gameObject);
        Object.Destroy(trans.Find("Progress").gameObject);

        var mainText = trans.Find("MainText");
        mainText.localPosition = new Vector3(-20, -18, 0);

        button.SetText("<color=white>P1</color>");

        var arrow = trans.Find("Arrow");

        arrow.localPosition = new Vector3(0, -40, 0);
        arrow.localScale = new Vector3(0.3f, 0.3f, 1);

        var lineBox = trans.Find("Box");
        lineBox.localPosition = new Vector3(0, -40, 0);

        for (int i = 0; i < lineBox.childCount; i++)
        {
            var child = lineBox.GetChild(i);

            switch (child.name)
            {
                case "StretchLineT":
                    child.localScale = new Vector3(0.22f, 4, 1);
                    child.localPosition = new Vector3(-45, 45, 0);
                    break;
                case "StretchLineB":
                    child.localScale = new Vector3(0.22f, 4, 1);
                    child.localPosition = new Vector3(-45, -45, 0);
                    break;
                case "StretchLineR":
                    child.localPosition = new Vector3(45, 45, 0);
                    break;
                case "StretchLineL":
                    child.localScale = new Vector3(0.5f, 1, 1);
                    child.localPosition = new Vector3(-45, 45, 0);
                    break;
            }
        }

        var boxCollider = button.GetComponent<BoxCollider2D>();

        boxCollider.size = new Vector2(80, 80);
        boxCollider.offset = new Vector2(0, -40);

        button.name = "LobbyExpander_SwitchLobbyButton";

        Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;

        go.SetActive(false);

        return go;
    }
}
