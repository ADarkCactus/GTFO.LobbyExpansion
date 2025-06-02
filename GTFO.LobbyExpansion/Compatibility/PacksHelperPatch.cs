// Credit to Dinorush for a large part of the fix.

using System.Linq;
using GTFO.LobbyExpansion.Util;
using HarmonyLib;

namespace GTFO.LobbyExpansion.Compatibility;

public class PacksHelperPatch : ModCompatibilityPatch
{
    public const string PluginGuid = "Localia.PacksHelper";

    #region Overrides of ModCompatibilityPatch

    public override bool ShouldApply()
    {
        return BepInExUtil.IsPluginLoaded(PluginGuid);
    }

    public override void Apply(Harmony harmony)
    {
        var pluginInstance = BepInExUtil.GetPluginInstance(PluginGuid);
        var types = pluginInstance.GetType().Assembly.GetTypes();
        var managerType = types.FirstOrDefault(t => t.Name.Equals("PH_Manager", StringComparison.OrdinalIgnoreCase));

        if (managerType is null)
        {
            L.Error($"Could not apply PacksHelper patch: {nameof(managerType)} was null.");

            return;
        }

        PatchManagerType(managerType);
    }

    #endregion

    protected static void PatchManagerType(Type managerType)
    {
        ReflectionUtil.SetStaticField(managerType, "health", new string[PluginConfig.MaxPlayers]);
        ReflectionUtil.SetStaticField(managerType, "standard", new string[PluginConfig.MaxPlayers]);
        ReflectionUtil.SetStaticField(managerType, "special", new string[PluginConfig.MaxPlayers]);
        ReflectionUtil.SetStaticField(managerType, "tool", new string[PluginConfig.MaxPlayers]);
        ReflectionUtil.SetStaticField(managerType, "ori_all_info", new string[PluginConfig.MaxPlayers]);
        ReflectionUtil.SetStaticField(managerType, "now_show_info", new string[PluginConfig.MaxPlayers]);
    }
}
