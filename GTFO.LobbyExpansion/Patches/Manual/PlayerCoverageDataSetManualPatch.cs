using AIGraph;
using GTFO.LobbyExpansion.Util;
using PlayerCoverage;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class PlayerCoverageDataSet_NodeManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(PlayerCoverageSystem.PlayerCoverageDataSet_Node);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                // TODO: This HAS TO be in a separate ManualPatch since it needs to run twice, and if we do it in the same patch it's going to patch the same address... Maybe we rework it?
                // TODO: This pattern matches both PlayerCoverageDataSet_Node$$.ctor and PlayerCoverageDataSet_Portal$$.ctor, but this is fine for now since we want to patch both anyway?...
                // for (int i = 0; i < 4; i++)
                /*
                 * (PlayerCoverageDataSet_Node$$.ctor)
                 * il2cpp:00000001806C4C1C                 inc     esi
                 * il2cpp:00000001806C4C1E                 cmp     esi, 4 <-- patch this
                 * il2cpp:00000001806C4C21                 jl      short loc_1806C4BB0
                 */
                Method = ".ctor",
                Description = "Patch PlayerCoverageDataSet_Node constructor and PlayerCoverageDataSet_Portal$$ constructor (#1)",
                Pattern = "FF C6 83 FE 04 7C 8D 48 8B 5C 24 30",
                Offset = 4,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // public PlayerCoverageSystem.PlayerCoverageData[] m_coverageDatas = new PlayerCoverageSystem.PlayerCoverageData[4];
                /*
                 * il2cpp:00000001806C4B73 loc_1806C4B73:                          ; CODE XREF: PlayerCoverage_PlayerCoverageSystem_PlayerCoverageDataSet_Node$$_ctor+1F↑j
                 * il2cpp:00000001806C4B73                 mov     rcx, cs:PlayerCoverage_PlayerCoverageSystem_PlayerCoverageData___TypeInfo ; PlayerCoverage.PlayerCoverageSystem.PlayerCoverageData[]_TypeInfo
                 * il2cpp:00000001806C4B7A                 mov     edx, 4 <-- patch this
                 * il2cpp:00000001806C4B7F                 call    createIl2CppArrayOfType
                 * il2cpp:00000001806C4B84                 lea     r14, [rbx+10h]
                 */
                Method = ".ctor",
                Description = $"Patch PlayerCoverageDataSet_Node constructor m_coverageDatas array size from 4 -> {PluginConfig.MaxPlayers}.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 4C 8D 73 10 48 8B D0 49 8B CE 49 89 06 E8 ?? ?? ?? ?? 33 D2",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // for (int i = 0; i < 4; i++)
                /*
                 * il2cpp:00000001806C48D4                 inc     ecx
                 * il2cpp:00000001806C48D6                 cmp     ecx, 4 <-- patch this
                 * il2cpp:00000001806C48D9                 jl      short loc_1806C48A7
                 */
                Method = nameof(PlayerCoverageSystem.PlayerCoverageDataSet_Node.GetNodeDistanceToClosestPlayer),
                Description = "Adjust GetNodeDistanceToClosestPlayer hardcoded iteration of 4.",
                Pattern = "83 F9 04 7C CC",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // for (int i = 0; i < 4; i++)
                /*
                 * il2cpp:00000001806C472A                 inc     ebx
                 * il2cpp:00000001806C472C                 cmp     ebx, 4 <-- patch this
                 * il2cpp:00000001806C472F                 jl      loc_1806C4690
                 */
                Method = nameof(PlayerCoverageSystem.PlayerCoverageDataSet_Node.GetNodeDistanceToClosestPlayer_Unblocked),
                Description = "Adjust GetNodeDistanceToClosestPlayer_Unblocked hardcoded iteration of 4.",
                Pattern = "83 FB 04 0F 8C 5B FF FF FF",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            }
        ]);
    }
}

public class PlayerCoverageDataSet_PortalManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(PlayerCoverageSystem.PlayerCoverageDataSet_Portal);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                // TODO: This HAS TO be in a separate ManualPatch since it needs to run twice, and if we do it in the same patch it's going to patch the same address... Maybe we rework it?
                // TODO: This pattern matches both PlayerCoverageDataSet_Node$$.ctor and PlayerCoverageDataSet_Portal$$.ctor, but this is fine for now since we want to patch both anyway?...
                // for (int i = 0; i < 4; i++)
                /*
                 * (PlayerCoverageDataSet_Portal$$.ctor)
                 * il2cpp:00000001806C4E3C                 inc     esi
                 * il2cpp:00000001806C4E3E                 cmp     esi, 4 <-- patch this
                 * il2cpp:00000001806C4E41                 jl      short loc_1806C4DD0
                 */
                Method = ".ctor",
                ParameterTypes = [typeof(AIG_CoursePortal)],
                Description = "Patch PlayerCoverageDataSet_Portal constructor (#1) for loop",
                Pattern = "FF C6 83 FE 04 7C 8D 48 8B 5C 24 30",
                Offset = 4,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // No r6 mono code equivalent
                /*
                 * il2cpp:00000001806C4DAA loc_1806C4DAA:                          ; CODE XREF: PlayerCoverage_PlayerCoverageSystem_PlayerCoverageDataSet_Portal$$_ctor+26↑j
                 * il2cpp:00000001806C4DAA                 mov     rcx, cs:PlayerCoverage_PlayerCoverageSystem_PlayerCoverageData___TypeInfo ; PlayerCoverage.PlayerCoverageSystem.PlayerCoverageData[]_TypeInfo
                 * il2cpp:00000001806C4DB1                 mov     edx, 4
                 * il2cpp:00000001806C4DB6                 call    createIl2CppArrayOfType
                 * il2cpp:00000001806C4DBB                 lea     r14, [rbx+10h]
                 */
                Method = ".ctor",
                ParameterTypes = [typeof(AIG_CoursePortal)],
                Description = $"Patch PlayerCoverageDataSet_Portal constructor m_coverageDatas array size from 4 -> {PluginConfig.MaxPlayers}.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 4C 8D 73 10 48 8B D0 49 8B CE 49 89 06 E8 ?? ?? ?? ?? 33 F6",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            }
        ]);
    }
}
