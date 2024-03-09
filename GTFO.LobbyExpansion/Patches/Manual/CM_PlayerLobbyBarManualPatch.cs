using CellMenu;
using GTFO.LobbyExpansion.Util;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class CM_PlayerLobbyBarManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(CM_PlayerLobbyBar);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                Method = nameof(CM_PlayerLobbyBar.UpdatePlayer),
                Description = "Adjust hardcoded limit of 4 in CM_PlayerLobbyBar::UpdatePlayer to new max players.",
                Pattern = "83 F8 04 0F 8D F0 00 00 00",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            }
        ]);
    }
}
