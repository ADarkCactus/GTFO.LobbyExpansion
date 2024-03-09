using GTFO.LobbyExpansion.Util;
using SNetwork;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class SNet_SyncManagerManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(SNet_SyncManager);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                Method = nameof(SNet_SyncManager.ValidateIndex),
                Description = "Remove SNet_SyncManager::ValidateIndex character index > 3 check, since we're using higher characterIDs.",
                Pattern = "0F 87 C8 06 00 00",
                Offset = 0,
                Bytes = GenerateNop(6)
            }
        ]);
    }
}
