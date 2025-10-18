using GTFO.LobbyExpansion.Util;
using SNetwork;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class SNet_PlayerSlotManagerManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(SNet_PlayerSlotManager);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                // for (int i = 0; i < 4; i++)
                /*
                 * il2cpp:0000000180F13918                 inc     edi
                 * il2cpp:0000000180F1391A                 cmp     edi, 4 <-- patch this
                 * il2cpp:0000000180F1391D                 jl      loc_180F137F4
                 * il2cpp:0000000180F13923                 jmp     short loc_180F13928
                 */
                Method = nameof(SNet_PlayerSlotManager.Internal_ManageSlot),
                Description = "Adjust Internal_ManageSlot hardcoded iteration of 4.",
                Pattern = "83 FF 04 0F 8C 95 FE FF FF",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // for (int i = 0; i < 4; i++)
                /*
                 * il2cpp:0000000180F13D3B                 inc     ebx
                 * il2cpp:0000000180F13D3D                 cmp     ebx, 4 <-- patch this
                 * il2cpp:0000000180F13D40                 jl      short loc_180F13CE0
                 */
                Method = nameof(SNet_PlayerSlotManager.OnResetSession),
                Description = "Adjust OnResetSession hardcoded iteration of 4.",
                Pattern = "83 FB 04 7C 9E",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // for (int i = 0; i < 4; i++)
                /*
                 * il2cpp:0000000180F1409D loc_180F1409D:                          ; CODE XREF: SNetwork_SNet_PlayerSlotManager$$OnValidateMasterData+15D↑j
                 * il2cpp:0000000180F1409D                                         ; SNetwork_SNet_PlayerSlotManager$$OnValidateMasterData+199↑j
                 * il2cpp:0000000180F1409D                 inc     edi
                 * il2cpp:0000000180F1409F                 cmp     edi, 4 <-- patch this
                 * il2cpp:0000000180F140A2                 jl      loc_180F13F10
                 */
                Method = nameof(SNet_PlayerSlotManager.OnValidateMasterData),
                Description = "Adjust OnValidateMasterData hardcoded iteration of 4.",
                Pattern = "83 FF 04 0F 8C 68 FE FF FF",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // for (int i = 0; i < 4; i++)
                /*
                 * il2cpp:0000000180F132D2                 inc     ebx
                 * il2cpp:0000000180F132D4                 cmp     ebx, 4 <-- patch this
                 * il2cpp:0000000180F132D7                 jl      short loc_180F13260
                 */
                Method = nameof(SNet_PlayerSlotManager.HasFreeBotSlot),
                Description = "Adjust HasFreeBotSlot hardcoded iteration of 4.",
                Pattern = "83 FB 04 7C 87",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // for (int i = 0; i < 4; i++)
                /*
                 * il2cpp:0000000180F1342D loc_180F1342D:                          ; CODE XREF: SNetwork_SNet_PlayerSlotManager$$HasFreeHumanSlot+65↑j
                 * il2cpp:0000000180F1342D                 inc     ebx
                 * il2cpp:0000000180F1342F                 cmp     ebx, 4
                 * il2cpp:0000000180F13432                 jl      loc_180F13370
                 */
                Method = nameof(SNet_PlayerSlotManager.HasFreeHumanSlot),
                Description = "Adjust HasFreeHumanSlot hardcoded iteration of 4.",
                Pattern = "83 FB 04 0F 8C 38 FF FF FF",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // for (int i = 0; i < 4; i++)
                /*
                 * il2cpp:0000000180F1465E                 inc     ebx
                 * il2cpp:0000000180F14660                 mov     [rcx+rax*4+20h], esi
                 * il2cpp:0000000180F14664                 cmp     ebx, 4 <-- patch this
                 * il2cpp:0000000180F14667                 jl      short loc_180F14630
                 */
                Method = nameof(SNet_PlayerSlotManager.SetAllPlayerSlotPermissions),
                Description = "Adjust SetAllPlayerSlotPermissions hardcoded iteration of 4.",
                Pattern = "83 FB 04 7C C7",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // for (int i = 0; i < 4; i++)
                /*
                 * il2cpp:0000000180F1308E                 inc     eax
                 * il2cpp:0000000180F13090                 cmp     eax, 4 <-- patch this
                 * il2cpp:0000000180F13093                 jl      short loc_180F1307F
                 */
                Method = nameof(SNet_PlayerSlotManager.AreSlotPermissionsSet),
                Description = "Adjust AreSlotPermissionsSet hardcoded iteration of 4.",
                Pattern = "83 F8 04 7C EA",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
        ]);
    }
}
