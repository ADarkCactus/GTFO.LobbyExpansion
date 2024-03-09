using GTFO.LobbyExpansion.Util;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class DiscordManagerManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(DiscordManager);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                Method = nameof(DiscordManager.GetMaxLobbySize),
                Description = "Adjust DiscordManager::GetMaxLobbySize hardcoded iteration to new max players.",
                Pattern = "BF 04 00 00 00 33 DB",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            }
        ]);
    }
}
