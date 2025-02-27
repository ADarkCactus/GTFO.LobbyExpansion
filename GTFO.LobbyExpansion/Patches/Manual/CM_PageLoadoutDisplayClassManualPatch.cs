using CellMenu;
using GTFO.LobbyExpansion.Util;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class CM_PageLoadoutDisplayClassManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(CM_PageLoadout.__c__DisplayClass38_1);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch()
            {
                Method = nameof(CM_PageLoadout.__c__DisplayClass38_1._ShowPermissionWindow_b__1),
                Description = "NOP out Il2Cpp check and assignment of CellSettingsManager.SettingsData.Player.LobbySlotPermissions[num] = this.permission;",
                Pattern = "48 85 C9 74 5C 3B 71 18 73 7D 48 63 C6 89 54 81 20",
                Offset = 0,
                Bytes = GenerateNop(17)
            }
        ]);
    }
}
