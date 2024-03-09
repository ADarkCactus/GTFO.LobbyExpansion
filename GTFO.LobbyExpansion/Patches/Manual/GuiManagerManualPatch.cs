using GTFO.LobbyExpansion.Util;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class GuiManagerManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(GuiManager);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                Method = nameof(GuiManager.Setup),
                Description = "Adjust GuiManager::Setup() m_playerPings = new SyncedNavMarkerWrapper[4] to new max players.",
                Pattern = "BA 04 00 00 00 4C 8B E8",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers],
                ScanSize = 7500 // big function
            }
        ]);
    }
}
