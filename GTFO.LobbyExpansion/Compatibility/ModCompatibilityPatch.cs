using HarmonyLib;

namespace GTFO.LobbyExpansion.Compatibility;

public abstract class ModCompatibilityPatch
{
    public abstract bool ShouldApply();

    public abstract void Apply(Harmony harmony);
}
