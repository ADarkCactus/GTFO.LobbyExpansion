// TODO: this is from the old version, cleanup and ensure this is needed.

using Gear;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(GearManager))]
public static class GearManagerPatch
{
    [HarmonyPatch(nameof(GearManager.Setup))]
    [HarmonyPostfix]
    public static void Setup__Postfix()
    {
        L.Verbose("GearManager::Setup postfix");

        var data = GearManager.BotFavoritesData;

        // god this is REALLY ugly

        if (data.LastEquipped_Melee.Length < PluginConfig.MaxPlayers)
        {
            var orig = data.LastEquipped_Melee;
            var updated = new Il2CppStringArray(PluginConfig.MaxPlayers);

            for (var i = 0; i < updated.Length; i++)
            {
                if (i < orig.Length)
                {
                    updated[i] = orig[i];
                    continue;
                }

                updated[i] = "";
            }

            data.LastEquipped_Melee = updated;
        }

        if (data.LastEquipped_Standard.Length < PluginConfig.MaxPlayers)
        {
            var orig = data.LastEquipped_Standard;
            var updated = new Il2CppStringArray(PluginConfig.MaxPlayers);

            for (var i = 0; i < updated.Length; i++)
            {
                if (i < orig.Length)
                {
                    updated[i] = orig[i];
                    continue;
                }

                updated[i] = "";
            }

            data.LastEquipped_Standard = updated;
        }

        if (data.LastEquipped_Special.Length < PluginConfig.MaxPlayers)
        {
            var orig = data.LastEquipped_Special;
            var updated = new Il2CppStringArray(PluginConfig.MaxPlayers);

            for (var i = 0; i < updated.Length; i++)
            {
                if (i < orig.Length)
                {
                    updated[i] = orig[i];
                    continue;
                }

                updated[i] = "";
            }

            data.LastEquipped_Special = updated;
        }

        if (data.LastEquipped_Class.Length < PluginConfig.MaxPlayers)
        {
            var orig = data.LastEquipped_Class;
            var updated = new Il2CppStringArray(PluginConfig.MaxPlayers);

            for (var i = 0; i < updated.Length; i++)
            {
                if (i < orig.Length)
                {
                    updated[i] = orig[i];
                    continue;
                }

                updated[i] = "";
            }

            data.LastEquipped_Class = updated;
        }

        if (data.LastEquipped_HackingTool.Length < PluginConfig.MaxPlayers)
        {
            var orig = data.LastEquipped_HackingTool;
            var updated = new Il2CppStringArray(PluginConfig.MaxPlayers);

            for (var i = 0; i < updated.Length; i++)
            {
                if (i < orig.Length)
                {
                    updated[i] = orig[i];
                    continue;
                }

                updated[i] = "";
            }

            data.LastEquipped_HackingTool = updated;
        }
    }
}
