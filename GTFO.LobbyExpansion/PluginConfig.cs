using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SNetwork;

namespace GTFO.LobbyExpansion;

public static class PluginConfig
{
    public const byte DEFAULT_MAX_PLAYERS = 8;
    private const string CONFIG_FILE_NAME = $"{nameof(LobbyExpansionConfig)}.json";
    private const string USER_CONFIG_FILE_NAME = $"{nameof(LobbyExpansionConfig)}_UserData.json";

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
    };

    private static readonly string _configPath = Path.Combine(BepInEx.Paths.ConfigPath, CONFIG_FILE_NAME);
    private static readonly string _configPathUserData = Path.Combine(BepInEx.Paths.ConfigPath, USER_CONFIG_FILE_NAME);

    internal static byte MaxPlayers = DEFAULT_MAX_PLAYERS;

    private static LobbyExpansionPlayerCount _configPlayerCount = new();
    private static LobbyExpansionConfig _configUserData = new();

    internal static void Load()
    {
        LoadCustomPlayerCount();

        if (!File.Exists(_configPathUserData))
        {
            SaveUserData();
        }

        var json = File.ReadAllText(_configPathUserData);
        _configUserData = JsonSerializer.Deserialize<LobbyExpansionConfig>(json, _serializerOptions) ?? new();

        _configUserData.SlotPermissions ??= new();
        _configUserData.CustomExtraBotNames ??= new();
    }

    private static void LoadCustomPlayerCount()
    {
        if (!File.Exists(_configPath))
        {
            return; // Do we even want/need to write this file?
            //File.WriteAllText(_configPath, JsonSerializer.Serialize(_configPlayerCount));
        }

        var json = File.ReadAllText(_configPath);
        _configPlayerCount = JsonSerializer.Deserialize<LobbyExpansionPlayerCount>(json, _serializerOptions) ?? new();

        if (_configPlayerCount.MaxPlayers < 4)
        {
            L.Error("ConfigFile: MaxPlayers must be greater or equal to '4'!");
            MaxPlayers = DEFAULT_MAX_PLAYERS;
            return;
        }

        L.Info($"Loaded custom player count from config file: {MaxPlayers} (default) -> {_configPlayerCount.MaxPlayers} (config)");
        MaxPlayers = _configPlayerCount.MaxPlayers;
    }

    private static void SaveUserData()
    {
        var json = JsonSerializer.Serialize(_configUserData, _serializerOptions);
        File.WriteAllText(_configPathUserData, json);
    }

    public static SNet_PlayerSlotManager.SlotPermission GetExtraSlotPermission(int slotIndex)
    {
        return _configUserData.SlotPermissions!.GetValueOrDefault(slotIndex, SNet_PlayerSlotManager.SlotPermission.Human);
    }

    public static void SetExtraLobbySlotPermissions(int slotIndexMinusOne, SNet_PlayerSlotManager.SlotPermission permission)
    {
        var slotIndex = slotIndexMinusOne + 1; // Game ignores local player => saves 0, 1, 2 for bots
        L.Info($"SetExtraLobbySlotPermissions called: slotIndex:{slotIndex}, SlotPermission:{permission}");
        _configUserData.SlotPermissions![slotIndex] = permission;
        SaveUserData();
    }

    public static string GetExtraSlotNickname(int characterIndex)
    {
        var index = characterIndex - 4;

        if (_configUserData.CustomExtraBotNames!.Count > 0 && index < _configUserData.CustomExtraBotNames.Count)
        {
            var name = _configUserData.CustomExtraBotNames[index];
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        return characterIndex switch
        {
            4 => "Schaeffer",
            5 => "North",
            6 => "Henriksson",
            7 => "Maddox",
            _ => $"Prisoner{characterIndex}"
        };
    }
}
