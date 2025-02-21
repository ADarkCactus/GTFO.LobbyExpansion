using System.Collections.Generic;
using SNetwork;

namespace GTFO.LobbyExpansion;

public class LobbyExpansionPlayerCount
{
    public byte MaxPlayers { get; set; } = PluginConfig.DEFAULT_MAX_PLAYERS;
}

public class LobbyExpansionConfig
{
    public Dictionary<int, SNet_PlayerSlotManager.SlotPermission>? SlotPermissions { get; set; } = new();
    public List<string>? CustomExtraBotNames { get; set; } = new();
}
