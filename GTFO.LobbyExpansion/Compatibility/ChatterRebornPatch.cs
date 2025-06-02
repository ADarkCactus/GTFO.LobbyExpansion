// Credit to Dinorush for a large part of the fix.

using System.Reflection;
using GTFO.LobbyExpansion.Util;
using HarmonyLib;

namespace GTFO.LobbyExpansion.Compatibility;

/**
 * TODO: There are more things to patch:
 *  - EnemyDramaBehavior::DisableEnemyDramaBehavior()
 *      (hardcoded for loop - for (int i = 0; i < 4; i++))
 */
public class ChatterRebornPatch : ModCompatibilityPatch
{
    public const string PluginGuid = "CHTR";

    public static ChatterRebornPatch Instance { get; } = new();

    private Type? _dramaChatterManagerType;

    private Type? _dramaChatterMachineType;

    private Type? _enemyDetectionManagerType;

    private Type? _enemyDramaBehaviorType;

    private FieldInfo? _m_playerVisiblityField;

    private FieldInfo? _m_detectedPlayers;

    private ChatterRebornPatch() { }

    #region Overrides of ModCompatibilityPatch

    public override bool ShouldApply()
    {
        return BepInExUtil.IsPluginLoaded(PluginGuid);
    }

    public override void Apply(Harmony harmony)
    {
        var pluginInstance = BepInExUtil.GetPluginInstance(PluginGuid);
        var assembly = pluginInstance.GetType().Assembly;

        _dramaChatterManagerType = ReflectionUtil.GetRequiredTypeByName("DramaChatterManager", assembly);
        _dramaChatterMachineType = ReflectionUtil.GetRequiredTypeByName("DramaChatterMachine", assembly);
        _enemyDetectionManagerType = ReflectionUtil.GetRequiredTypeByName("EnemyDetectionManager", assembly);
        _enemyDramaBehaviorType = ReflectionUtil.GetRequiredTypeByName("EnemyDramaBehavior", assembly);

        _m_playerVisiblityField = AccessTools.Field(_enemyDramaBehaviorType, "m_playerVisiblity");
        L.Assert(_m_playerVisiblityField is not null, $"{nameof(_m_playerVisiblityField)} is null.");

        _m_detectedPlayers = AccessTools.Field(_enemyDramaBehaviorType, "m_detectedPlayers");
        L.Assert(_m_detectedPlayers is not null, $"{nameof(_m_detectedPlayers)} is null.");

        FixFieldsAndProperties();
        ApplyPatches(harmony);
    }

    #endregion

    protected void FixFieldsAndProperties()
    {
        // TODO: Clean this up later.

        var detectionManagerCurrentProperty = _enemyDetectionManagerType!.GetProperty(
            "Current",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (detectionManagerCurrentProperty is null)
            throw new Exception($"Unable to find {nameof(detectionManagerCurrentProperty)}.");

        var detectionManager = detectionManagerCurrentProperty.GetValue(null);

        if (detectionManager is null)
            throw new Exception($"{nameof(detectionManager)} current instance was null.");

        var dramaChatterManagerCurrentProperty = _dramaChatterManagerType!.GetProperty(
            "Current",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (dramaChatterManagerCurrentProperty is null)
            throw new Exception($"Unable to find {nameof(dramaChatterManagerCurrentProperty)} in {nameof(_dramaChatterManagerType)}.");

        var dramaChatterManager = dramaChatterManagerCurrentProperty.GetValue(null);

        if (dramaChatterManager is null)
            throw new Exception($"{nameof(dramaChatterManager)} current instance was null.");

        var playerDramaMachinesProperty = _dramaChatterManagerType.GetProperty(
            "PlayerDramaMachines",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (playerDramaMachinesProperty is null)
            throw new Exception("Unable to find PlayerDramaMachines property");

        // Expand the array sizes to accommodate the maximum number of players
        var expandedChatterMachineArray = Array.CreateInstance(_dramaChatterMachineType!, PluginConfig.MaxPlayers);
        playerDramaMachinesProperty.SetValue(dramaChatterManager, expandedChatterMachineArray);

        var allowParticipationField = _dramaChatterManagerType.GetField(
            "_allow_participation",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (allowParticipationField is null)
            throw new Exception("Unable to find _allow_participation field");

        allowParticipationField.SetValue(dramaChatterManager, new bool[PluginConfig.MaxPlayers]);

        var m_enemyScoresField = _enemyDetectionManagerType.GetField(
            "m_enemyScores",
            BindingFlags.Public | BindingFlags.Instance);

        if (m_enemyScoresField is null)
            throw new Exception($"Unable to find {nameof(m_enemyScoresField)}.");

        m_enemyScoresField.SetValue(detectionManager, new float[PluginConfig.MaxPlayers]);

        // Typo in m_enemyVisibilites is on purpose.
        var m_enemyVisibilitesField = _enemyDetectionManagerType.GetField(
            "m_enemyVisibilites",
            BindingFlags.Public | BindingFlags.Instance);

        if (m_enemyVisibilitesField is null)
            throw new Exception($"Unable to find {nameof(m_enemyVisibilitesField)}.");

        m_enemyVisibilitesField.SetValue(detectionManager, new int[PluginConfig.MaxPlayers]);
    }

    protected void ApplyPatches(Harmony harmony)
    {
        harmony.PatchAll(typeof(ChatterRebornPatch));

        var original = AccessTools.Method(_enemyDramaBehaviorType, "Awake");
        var postfix = AccessTools.Method(typeof(ChatterRebornPatch), nameof(EnemyDramaBehavior__Awake__Postfix));
        harmony.Patch(original, postfix: new HarmonyMethod(postfix));

        L.Debug($"Applied patches in {nameof(ChatterRebornPatch)}.");
    }

    [HarmonyPatch(typeof(StartMainGame), nameof(StartMainGame.Start))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    private static void StartMainGame__Start__Postfix()
    {
        L.LogExecutingMethod();

        // TODO: This might not actually be needed.
        Instance.FixFieldsAndProperties();
    }

    // Patched manually since we don't have a reference to the EnemyDramaBehavior type at compile time.
    [HarmonyWrapSafe]
    private static void EnemyDramaBehavior__Awake__Postfix(object __instance)
    {
        L.LogExecutingMethod();

        if (Instance._enemyDramaBehaviorType is null)
        {
            L.Error($"Awake postfix can't run; {nameof(_enemyDramaBehaviorType)} is null.");

            return;
        }

        if (!Instance._enemyDramaBehaviorType.IsInstanceOfType(__instance))
        {
            L.Error(
                $"Awake postfix was passed an instance that is NOT an instance of {nameof(_enemyDramaBehaviorType)} type? It was of type {__instance.GetType().FullName}");

            return;
        }

        if (Instance._m_playerVisiblityField is null)
        {
            L.Error($"Awake postfix can't run; {nameof(_m_playerVisiblityField)} is null.");

            return;
        }

        if (Instance._m_detectedPlayers is null)
        {
            L.Error($"Awake postfix can't run; {nameof(_m_detectedPlayers)} is null.");

            return;
        }

        Instance._m_playerVisiblityField.SetValue(__instance, new float[PluginConfig.MaxPlayers]);
        Instance._m_detectedPlayers.SetValue(__instance, new bool[PluginConfig.MaxPlayers]);
    }
}
