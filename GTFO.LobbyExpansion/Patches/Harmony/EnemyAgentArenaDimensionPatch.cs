using Enemies;
using HarmonyLib;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(EnemyAgent))]
public class EnemyAgentArenaDimensionPatch
{
    [HarmonyPatch(nameof(EnemyAgent.GetArenaDimension))]
    [HarmonyPrefix]
    public static void GetArenaDimension__Prefix(ref uint slotIndex)
    {
        slotIndex %= 4;
    }
}
