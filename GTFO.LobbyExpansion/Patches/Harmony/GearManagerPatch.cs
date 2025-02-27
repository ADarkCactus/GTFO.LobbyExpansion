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

        var gearMelee = data.LastEquipped_Melee;
        if (Expand(ref gearMelee))
            data.LastEquipped_Melee = gearMelee;

        var gearStandard = data.LastEquipped_Standard;
        if (Expand(ref gearStandard))
            data.LastEquipped_Standard = gearStandard;

        var gearSpecial = data.LastEquipped_Special;
        if (Expand(ref gearSpecial))
            data.LastEquipped_Special = gearSpecial;

        var gearClass = data.LastEquipped_Class;
        if (Expand(ref gearClass))
            data.LastEquipped_Class = gearClass;

        var hackingTool = data.LastEquipped_HackingTool;
        if (Expand(ref hackingTool))
            data.LastEquipped_HackingTool = hackingTool;
    }

    private static bool Expand(ref Il2CppStringArray original)
    {
        if (original.Length >= PluginConfig.MaxPlayers)
        {
            return false;
        }

        var orig = original;
        var updated = new Il2CppStringArray(PluginConfig.MaxPlayers);

        for (var i = 0; i < updated.Length; i++)
        {
            if (i < orig.Length)
            {
                updated[i] = orig[i];
                continue;
            }

            updated[i] = string.Empty;
        }

        original = updated;
        return true;
    }
}
