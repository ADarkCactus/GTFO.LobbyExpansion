namespace GTFO.LobbyExpansion;

public class PatchingException : Exception
{
    public PatchingException(string? message) : base(message)
    {
    }

    public PatchingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
