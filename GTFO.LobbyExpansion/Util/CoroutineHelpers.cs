using System.Collections;

namespace GTFO.LobbyExpansion.Util;

public static class CoroutineHelpers
{
    public static IEnumerator NextFrame(Action action)
    {
        yield return null;

        action?.Invoke();
    }
}
