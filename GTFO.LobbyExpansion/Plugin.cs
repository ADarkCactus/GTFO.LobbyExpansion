using System.Collections.Generic;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using GTFO.LobbyExpansion.Patches.Manual;
using GTFO.LobbyExpansion.Util;
using HarmonyLib;

namespace GTFO.LobbyExpansion;

[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BasePlugin
{
    private Harmony _harmony = null!;

    public override void Load()
    {
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
                new SNet_SyncManagerManualPatch()
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

        L.Info($"Loaded plugin {PluginInfo.Name}.");
    }
}
