using CellMenu;
using GTFO.LobbyExpansion.Util;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class CM_PageExpeditionSuccessManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(CM_PageExpeditionSuccess);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                Method = nameof(CM_PageExpeditionSuccess.Setup),
                Description = "Adjust hardcoded m_playerReports size in Setup.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8D B7 F0 01 00 00",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            }
        ]);
    }
}
