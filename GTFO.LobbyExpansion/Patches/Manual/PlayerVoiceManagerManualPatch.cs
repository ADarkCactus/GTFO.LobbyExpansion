using GTFO.LobbyExpansion.Util;
using Player;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class PlayerVoiceManagerManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(PlayerVoiceManager);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                Method = nameof(PlayerVoiceManager.RegisterPlayerVoice),
                Description = "Patch PlayerVoiceManager < 3",
                Pattern = "83 FE 03 76 67",
                Offset = 2,
                Bytes = [(byte)(PluginConfig.MaxPlayers - 1)]
            }
        ]);
    }
}
