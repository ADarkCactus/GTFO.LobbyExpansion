using GTFO.LobbyExpansion.Util;

namespace GTFO.LobbyExpansion;

public class PatchingException : Exception
{
    public readonly ManualPatch PatchOrigin;

    public PatchingException(string? message, ManualPatch origin) : base(message)
    {
        PatchOrigin = origin;
    }

    public PatchingException(string? message, ManualPatch origin, Exception? innerException) : base(message, innerException)
    {
        PatchOrigin = origin;
    }
}
