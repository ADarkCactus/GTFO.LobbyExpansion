using GTFO.LobbyExpansion.Util;
using Player;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class PlayerSessionStatusManagerManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(PlayerSessionStatusManager);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                Method = nameof(PlayerSessionStatusManager.OnPlayerStateCapture),
                Description = "Adjust PlayerSessionStatusManager::OnPlayerStateCapture() bool flag2 = characterIndex > -1 && characterIndex < 4; to new max players.",
                Pattern = "83 F8 04 0F 9C C3",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            }
        ]);
    }
}
