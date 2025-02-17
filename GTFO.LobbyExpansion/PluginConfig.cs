using SNetwork;

namespace GTFO.LobbyExpansion;

public static class PluginConfig
{
    public static readonly byte MaxPlayers = 8;

    public static SNet_PlayerSlotManager.SlotPermission GetExtraSlotPermission(int slotIndex)
    {
        // Probably worth making this a setting instead of people having to reset it every time.
        return SNet_PlayerSlotManager.SlotPermission.Human;
    }

    public static void SetExtraLobbySlotPermissions(int slotIndex, SNet_PlayerSlotManager.SlotPermission permission)
    {
        L.Warning($"SetExtraLobbySlotPermissions called: slotIndex:{slotIndex}, SlotPermission:{permission}");
        // TODO: Implement this
    }

    public static string GetExtraSlotNickname(int characterIndex)
    {
        return characterIndex switch
        {
            4 => "Schaeffer",
            5 => "North",
            6 => "Henriksson",
            7 => "Maddox",
            _ => $"Prisoner{characterIndex}"
        };
    }
}
