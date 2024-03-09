using GTFO.LobbyExpansion.Util;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class PLOC_InElevatorManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(PLOC_InElevator);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                Method = nameof(PLOC_InElevator.CommonEnter),
                Description = "PLOC_InElevator::CommonEnter PLOCStateReferenceID > 3",
                Pattern = "83 B8 A0 00 00 00 03",
                Offset = 6,
                Bytes = [(byte)(PluginConfig.MaxPlayers - 1)]
            }
        ]);
    }
}
