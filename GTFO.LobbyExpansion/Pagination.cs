using CellMenu;

namespace GTFO.LobbyExpansion;

public class Pagination
{
    public int PageIndex { get; set; }

    public event Action? OnPageChanged;

    public bool CanPageUp()
    {
        var maxPageIndex = (int)Math.Ceiling(PluginConfig.MaxPlayers / 4d) - 1;
        return PageIndex < maxPageIndex;
    }

    public bool PageUp()
    {
        if (CanPageUp())
        {
            PageIndex++;
            OnPageChanged?.Invoke();
            return true;
        }

        return false;
    }

    public bool CanPageDown()
    {
        return PageIndex > 0;
    }

    public bool PageDown()
    {
        if (CanPageDown())
        {
            PageIndex--;
            OnPageChanged?.Invoke();
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

    public static int GetPageFromSlotIndex(int slotIndex)
    {
        return (int) Math.Floor(slotIndex / 4f);
    }
}
