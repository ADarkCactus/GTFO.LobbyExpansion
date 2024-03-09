using System.Diagnostics;
using BepInEx.Logging;

namespace GTFO.LobbyExpansion;

internal static class L
{
    private static readonly ManualLogSource Logger = new(PluginInfo.Name);

    static L() => BepInEx.Logging.Logger.Sources.Add(Logger);

    internal static void Assert(bool condition, object data)
    {
        if (!condition)
        {
            var message = $"ASSERTION FAILED: {Format(data)}";
            Fatal(message);
            throw new ApplicationException(message);
        }
    }

    internal static void DebugWarning(object data)
    {
#if DEBUG
        Warning("-------------------------------------------");
        Warning("-------------- DEBUG WARNING --------------");
        Warning(Format(data));
        Warning("-------------------------------------------");
#endif
    }

    internal static void Info(object data) => Logger.LogMessage(Format(data));

    internal static void Verbose(object data) => Logger.LogDebug(Format(data));

    internal static void Error(object data) => Logger.LogError(Format(data));

    internal static void Fatal(object data) => Logger.LogFatal(Format(data));

    internal static void Warning(object data) => Logger.LogWarning(Format(data));

    internal static void LogExecutingMethod(string? parameterInfo = "")
    {
        var executingMethod = new StackFrame(1, false).GetMethod();

        if (executingMethod == null)
        {
            Error("COULD NOT LOG EXECUTING METHOD. THIS SHOULD NOT HAPPEN.");
            return;
        }

        var className = executingMethod.DeclaringType?.FullName ?? "???";
        var methodName = executingMethod.Name;
        var formattedParameterInfo = string.IsNullOrWhiteSpace(parameterInfo) ? "" : $" ({parameterInfo})";
        Verbose($"{className}.{methodName}{formattedParameterInfo}");
    }

    private static string Format(object msg) => msg.ToString() ?? "";
}
