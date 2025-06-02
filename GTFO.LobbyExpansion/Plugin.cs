using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using GTFO.LobbyExpansion.Compatibility;
using GTFO.LobbyExpansion.Patches.Manual;
using GTFO.LobbyExpansion.Util;
using HarmonyLib;

namespace GTFO.LobbyExpansion;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
[BepInDependency(ChatterRebornPatch.PluginGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(PacksHelperPatch.PluginGuid, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    private Harmony _harmony = null!;

    public override void Load()
    {
        try
        {
            PluginConfig.Load();
        }
        catch (Exception e)
        {
            L.Error("Failed to load plugin config?!");
            L.Error($"{e.GetType().Name}: {e.Message}");
            L.Warning($"StackTrace:\n{e.StackTrace}");
        }

        try
        {
#if DEBUG
            // TODO: Remove this later, just helps find it easier in BepInEx console
            L.Warning("------------------------------------------------------");
            L.Warning("------------------------------------------------------");
            L.Warning("------------------------------------------------------");
            L.Warning("------------------------------------------------------");
            L.Warning("-------------- Applying manual patches. --------------");
            L.Warning("------------------------------------------------------");
            L.Warning("------------------------------------------------------");
            L.Warning("------------------------------------------------------");
            L.Warning("------------------------------------------------------");
#endif

            var appliedPatches = new HashSet<Type>();
            var manualPatches = new List<ManualPatch>
            {
                new CM_PageExpeditionSuccessManualPatch(),
                new CM_PageLoadoutManualPatch(),
                new CM_PageMapManualPatch(),
                new CM_PlayerLobbyBarManualPatch(),
                new DiscordManagerManualPatch(),
                new GuiManagerManualPatch(),
                new PLOC_InElevatorManualPatch(),
                new PlayerAgentManualPatch(),
                new PlayerBotAIDataManualPatch(),
                new PlayerCoverageDataSet_NodeManualPatch(),
                new PlayerCoverageDataSet_PortalManualPatch(),
                new PlayerManagerManualPatch(),
                new PlayerSessionStatusManagerManualPatch(),
                new PlayerVoiceManagerManualPatch(),
                new SNet_PlayerSlotManagerManualPatch(),
                new SNet_SyncManagerManualPatch(),
                new CM_PageLoadoutDisplayClassManualPatch()
            };

            foreach (var patch in manualPatches)
            {
                var patchType = patch.GetType();

                if (appliedPatches.Contains(patchType))
                {
                    var message = $"The same patch {patchType} is being applied twice. Fix this.";
                    L.Fatal(message);
                    throw new PatchingException(message);
                }

                L.Verbose($"Applying manual patch {patch.GetType().Name}.");
                patch.Apply();
                appliedPatches.Add(patchType);
            }
        }
        catch (Exception e)
        {
            L.Fatal("An error occurred while applying manual patches:");
            L.Fatal(e);
            return;
        }

#if DEBUG
        // TODO: Remove this later, just helps find it easier in BepInEx console
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("-------------- Applying Harmony patches. -------------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
#endif

        try
        {
            _harmony = new Harmony(PluginInfo.Guid);
            _harmony.PatchAll();
        }
        catch (Exception e)
        {
            L.Fatal("An error occurred while applying patches:");
            L.Fatal(e);
            return;
        }

#if DEBUG
        // TODO: Remove this later, just helps find it easier in BepInEx console
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("--------- Applying mod compatibility patches. --------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
        L.Warning("------------------------------------------------------");
#endif

        var modCompatibilityPatches = new List<ModCompatibilityPatch>()
        {
            new PacksHelperPatch(),
            ChatterRebornPatch.Instance
        };

        foreach (var patch in modCompatibilityPatches)
        {
            var patchName = patch.GetType().Name;

            try
            {
                if (!patch.ShouldApply())
                {
                    L.Verbose($"Skipping initialization of {patchName} since the patch deemed it shouldn't be applied.");
                    return;
                }

                L.Info($"Applying mod compatibility patch {patchName}.");
                patch.Apply(_harmony);
            }
            catch (Exception e)
            {
                L.Fatal($"An error occurred while applying mod compatibility patch {patchName}:");
                L.Fatal(e);
                return;
            }
        }

        L.Info($"Loaded plugin {PluginInfo.Name}.");
    }
}
