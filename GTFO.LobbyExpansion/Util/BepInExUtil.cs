using BepInEx.Unity.IL2CPP;

namespace GTFO.LobbyExpansion.Util;

public static class BepInExUtil
{
    public static bool IsPluginLoaded(string pluginGuid)
    {
        return IL2CPPChainloader.Instance.Plugins.ContainsKey(pluginGuid);
    }

    public static object GetPluginInstance(string pluginGuid)
    {
        if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(pluginGuid, out var pluginInfo))
        {
            throw new InvalidOperationException($"Could not retrieve plugin info for plugin with GUID {pluginGuid}.");
        }

        var instance = pluginInfo.Instance;

        if (instance is null)
        {
            throw new InvalidOperationException($"Plugin with GUID {pluginGuid} had a null instance.");
        }

        return instance;
    }
}
