using GTFO.LobbyExpansion.Util;
using Player;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class PlayerAgentManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(PlayerAgent);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                Method = nameof(PlayerAgent.Setup),
                Description = "Remove characterID limit in PlayerAgent::Setup",
                Pattern = "0F 87 2C 10 00 00",
                Offset = 0,
                Bytes = GenerateNop(6)
            },
            new Patch
            {
                Method = nameof(PlayerAgent.Setup),
                Description = "Remove characterID >= m_modelsForSync length in PlayerAgent::Setup since m_modelsForSync isn't used anyway",
                Pattern = "44 3B 70 18 0F 8D 12 10 00 00",
                Offset = 4,
                Bytes = GenerateNop(6)
            }
        ]);
    }
}
