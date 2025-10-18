using GTFO.LobbyExpansion.Util;
using Player;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class PlayerManagerManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(PlayerManager);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                // if (SNet.Slots.SlottedPlayers.Count == 4)
                /*
                 * il2cpp:00000001809CA91A                 cmp     dword ptr [rax+18h], 4
                 * il2cpp:00000001809CA91E                 jz      loc_1809CACE4 <-- NOP this
                 */
                Method = nameof(PlayerManager.SpawnBot),
                Description = "Remove SpawnBot hardcoded limit of 4.",
                Pattern = "83 78 18 04 0F 84 9D 03 00 00",
                Offset = 4,
                Bytes = GenerateNop(6)
            },
            new Patch
            {
                // private bool[] m_botSlots = new bool[4];
                /*
                 * il2cpp:00000001809CE464                 mov     rcx, cs:bool___TypeInfo ; bool[]_TypeInfo
                 * il2cpp:00000001809CE46B                 mov     edx, 4 <-- patch this to new limit
                 * il2cpp:00000001809CE470                 call    createIl2CppArrayOfType
                 */
                Method = ".ctor",
                Description = "Set constructor m_botSlot size to new max player limit.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8D 4E 70",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // TODO: This pattern is a joke. Rework it or convert it to a Harmony patch.
                // TODO: Do we even have the patch PlayerBotAIData's constructor then?
                // This is the inlined version of the PositionReservations construction, but inside the PlayerManager constructor
                Method = ".ctor",
                Description = "Patch inlined version of the creation of PositionReservations in PlayerManager constructor.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8D 4F 20 48 8B D0 48 89 01 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8D 4F 28 48 8B D0 48 89 01 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 8B C8 48 8B D8 E8 ?? ?? ?? ?? 48 8D 4F 30 48 8B D3 48 89 19 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 8B C8 48 8B D8 E8 ?? ?? ?? ?? 48 8D 4F 38 48 8B D3 48 89 19 E8 ?? ?? ?? ?? 33 D2 48 8B CF E8",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // TODO: This pattern is a joke. Rework it or convert it to a Harmony patch.
                // TODO: Do we even have the patch PlayerBotAIData's constructor then?
                // This is the inlined version of the ObjectReservations construction, but inside the PlayerManager constructor
                Method = ".ctor",
                Description = "Patch inlined version of the creation of ObjectReservations in PlayerManager constructor.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8D 4F 28 48 8B D0 48 89 01 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 8B C8 48 8B D8 E8 ?? ?? ?? ?? 48 8D 4F 30 48 8B D3 48 89 19 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 8B C8 48 8B D8 E8 ?? ?? ?? ?? 48 8D 4F 38 48 8B D3 48 89 19 E8 ?? ?? ?? ?? 33 D2 48 8B CF E8",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
        ]);
    }
}

public class PlayerBotAIDataManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(PlayerManager.PlayerBotAIData);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                // TODO: This pattern is a joke. Rework it or convert it to a Harmony patch.
                // public List<PlayerManager.PositionReservation>[] PositionReservations = new List<PlayerManager.PositionReservation>[4];
                /*
                 * il2cpp:00000001809BEDD2                 mov     rcx, cs:System_Collections_Generic_List_PlayerManager_PositionReservation____TypeInfo ; System.Collections.Generic.List<PlayerManager.PositionReservation>[]_TypeInfo
                 * il2cpp:00000001809BEDD9                 mov     edx, 4 <-- patch this to new limit
                 * il2cpp:00000001809BEDDE                 call    createIl2CppArrayOfType
                 */
                Method = ".ctor",
                Description = "Set PlayerBotAIData constructor PositionReservations size to new max player limit.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8D 4F 20 48 8B D0 48 89 01 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8D 4F 28 48 8B D0 48 89 01 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 8B C8 48 8B D8 E8 ?? ?? ?? ?? 48 8D 4F 30 48 8B D3 48 89 19 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 8B C8 48 8B D8 E8 ?? ?? ?? ?? 48 8D 4F 38 48 8B D3 48 89 19 E8 ?? ?? ?? ?? 33 D2 48 8B CF 48 8B 5C 24 30",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                // TODO: This pattern is a joke. Rework it or convert it to a Harmony patch.
                // public List<PlayerManager.ObjectReservation>[] ObjectReservations = new List<PlayerManager.ObjectReservation>[4];
                /*
                 * il2cpp:00000001809BEDF2                 mov     rcx, cs:System_Collections_Generic_List_PlayerManager_ObjectReservation____TypeInfo ; System.Collections.Generic.List<PlayerManager.ObjectReservation>[]_TypeInfo
                 * il2cpp:00000001809BEDF9                 mov     edx, 4 <-- patch this to new limit
                 * il2cpp:00000001809BEDFE                 call    createIl2CppArrayOfType
                 */
                Method = ".ctor",
                Description = "Set PlayerBotAIData constructor ObjectReservations size to new max player limit.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8D 4F 28 48 8B D0 48 89 01 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 8B C8 48 8B D8 E8 ?? ?? ?? ?? 48 8D 4F 30 48 8B D3 48 89 19 E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 15 ?? ?? ?? ?? 48 8B C8 48 8B D8 E8 ?? ?? ?? ?? 48 8D 4F 38 48 8B D3 48 89 19 E8 ?? ?? ?? ?? 33 D2 48 8B CF 48 8B 5C 24 30",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
        ]);
    }
}
