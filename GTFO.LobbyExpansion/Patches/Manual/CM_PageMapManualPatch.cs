using CellMenu;
using GTFO.LobbyExpansion.Util;

namespace GTFO.LobbyExpansion.Patches.Manual;

public class CM_PageMapManualPatch : ManualPatch
{
    protected override Type TargetType { get; } = typeof(CM_PageMap);

    protected override void SetupPatches()
    {
        Patches.AddRange([
            new Patch
            {
                // TODO: this could probably just be done as a harmony patch
                Method = nameof(CM_PageMap.TryGetInventoryWithSlotIndex),
                Description = "Remove CM_PageMap::TryGetInventoryWithSlotIndex slotIndex > 3",
                Pattern = "83 FF 03 77 60",
                Offset = 2,
                Bytes = [(byte)(PluginConfig.MaxPlayers - 1)]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.Setup),
                Description = "Adjust CM_PageMap::Setup() m_inventory = new PUI_Inventory[4] to new max players.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 4C 8D A7 38 02 00 00",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.CreatePlayerIcons),
                Description = "Adjust CM_PageMap::CreatePlayerIcons() m_syncedPlayers = new CM_MapPlayerGUIItem[4] to new max players.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 49 8D AF 50 02 00 00",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.CreatePlayerIcons),
                Description = "Adjust CM_PageMap::CreatePlayerIcons() m_syncedCursors = new CM_Cursor[4] to new max players.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 4D 8D B7 58 02 00 00",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.CreatePlayerIcons),
                Description = "Adjust CM_PageMap::CreatePlayerIcons() m_drawPixelBufferIndex = new int[4] to new max players.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 49 8D 8F 80 02 00 00",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.CreatePlayerIcons),
                Description = "Adjust CM_PageMap::CreatePlayerIcons() m_drawPixelBuffer = new CM_MapDrawPixel[4][] to new max players.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8B D0 49 89 87 78 02 00 00",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.CreatePlayerIcons),
                Description = "Adjust CM_PageMap::CreatePlayerIcons() m_lastDrawingPos = new Vector2[4] to new max players.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 49 8D 8F 68 02 00 00",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.CreatePlayerIcons),
                Description = "Adjust CM_PageMap::CreatePlayerIcons() for (int i = 0; i < 4; i++) to new max players.",
                Pattern = "83 FE 04 0F 8C A6 FC FF FF",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.UpdatePlayerData),
                Description = "Adjust CM_PageMap::UpdatePlayerData() for (int i = 0; i < 4; i++) to new max players.",
                Pattern = "83 FB 04 0F 8C 1B FF FF FF",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.UpdateSyncedCursorVisibility),
                Description = "Adjust CM_PageMap::UpdateSyncedCursorVisibility() for (int i = 0; i < 4; i++) to new max players.",
                Pattern = "83 FB 04 0F 8C 61 FF FF FF",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.Update),
                Description = "Adjust CM_PageMap::UpdateSyncedCursorVisibility() (Inlined into Update()!) for (int i = 0; i < 4; i++) to new max players.",
                Pattern = "83 FB 04 0F 8C 55 FF FF FF",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.SetPageActive),
                Description = "Adjust CM_PageMap::SetPageActive() m_playerIsDrawing = new bool[4] to new max players.",
                Pattern = "BA 04 00 00 00 E8 ?? ?? ?? ?? 48 8D 8B 60 02 00 00",
                Offset = 1,
                Bytes = [PluginConfig.MaxPlayers]
            },
            new Patch
            {
                Method = nameof(CM_PageMap.OnEnable),
                Description = "Adjust CM_PageMap::OnEnable() for (int i = 0; i < 4; i++) to new max players.",
                Pattern = "83 FE 04 7C 94",
                Offset = 2,
                Bytes = [PluginConfig.MaxPlayers]
            },
        ]);
    }
}
