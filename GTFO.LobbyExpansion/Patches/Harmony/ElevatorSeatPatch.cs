using BepInEx.Unity.IL2CPP.Utils.Collections;
using GTFO.LobbyExpansion.Util;
using HarmonyLib;
using UnityEngine;

namespace GTFO.LobbyExpansion.Patches.Harmony;

[HarmonyPatch(typeof(ElevatorSeat))]
public class ElevatorSeatPatch
{
    private static int _frame;

    [HarmonyPatch(nameof(ElevatorSeat.SkipPreReleaseSequence))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void SkipPreReleaseSequence__Postfix(CellSoundPlayer sound)
    {
        sound.Stop();

        var currentFrame = Time.frameCount;

        if (_frame != currentFrame)
        {
            // Play sound *once*, a frame later - should be unnoticeable
            CoroutineManager.StartCoroutine(CoroutineHelpers.NextFrame(() =>
            {
                sound.Post(AK.EVENTS.PLAY_15_ELEVATOR_RELEASE, isGlobal: true);
            }).WrapToIl2Cpp());
        }

        _frame = currentFrame;
    }
}
