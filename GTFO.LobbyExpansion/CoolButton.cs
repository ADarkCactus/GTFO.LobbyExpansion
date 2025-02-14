using System.Linq;
using CellMenu;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GTFO.LobbyExpansion;

public static class CoolButton
{
    private const float ALPHA_OVER = 1f;
    private const float ALPHA_OVER_DISABLED = 0.25f;

    private const float ALPHA_OUT = 0.66f;
    private const float ALPHA_OUT_DISABLED = 0.25f;

    private static readonly Color COLOR_OUT = new(1, 1, 1, 0.66f);
    private static readonly Color COLOR_HOVER = new(1, 1, 1, 1f);
    private static readonly Color COLOR_DISABLED = new(1, 1, 1, 0.25f);

    public static void SetCoolButtonEnabled(this CM_Item button, bool enabled)
    {
        if (button == null)
            return;

        button.m_spriteAlphaOut = enabled ? ALPHA_OUT : ALPHA_OUT_DISABLED;
        button.m_spriteAlphaOver = enabled ? ALPHA_OVER : ALPHA_OVER_DISABLED;

        button.m_spriteColorOut = enabled ? COLOR_OUT : COLOR_DISABLED;
        button.m_spriteColorOver = enabled ? COLOR_HOVER : COLOR_DISABLED;

        button.OnHoverOut();

        button.m_collider.enabled = enabled;
    }

    private static GameObject _squareButtonPrefab = null!;

    internal static void Setup(CM_PageLoadout pageLoadout)
    {
        if (_squareButtonPrefab != null)
            return;

        _squareButtonPrefab = CreateButtonPrefab(pageLoadout);
    }

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

    private static GameObject CreateButtonPrefab(CM_PageLoadout pageLoadout)
    {
        var button = Object.Instantiate(pageLoadout.m_readyButtonPrefab).GetComponent<CM_Item>();
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
