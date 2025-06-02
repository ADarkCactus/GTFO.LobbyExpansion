using System.Reflection;
using System.Runtime.CompilerServices;

namespace GTFO.LobbyExpansion.Util;

public static class ReflectionUtil
{
    public static void SetStaticField(Type type, string fieldName, object value)
    {
        var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        if (field == null)
        {
            throw new ArgumentException(
                $"Could not find static field '{fieldName}' in type '{type.FullName}'.",
                nameof(fieldName));
        }

        field.SetValue(null, value);
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    public static Type GetRequiredTypeByName(string name, Assembly assembly)
    {
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            if (type.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return type;
            }
        }

        throw new ArgumentException($"Could not find type '{name}' in assembly '{assembly.FullName ?? "unnamed"}'.");
    }
}
