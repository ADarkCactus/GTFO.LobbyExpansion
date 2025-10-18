using System.Collections.Generic;
using System.Linq;

namespace GTFO.LobbyExpansion.Util;

public abstract class ManualPatch
{
    private static readonly HashSet<IntPtr> PatchedAddresses = [];
    protected List<Patch> Patches { get; } = [];
    protected abstract Type TargetType { get; }

    public IEnumerable<PatchingException> Apply()
    {
        SetupPatches();

        foreach (var patch in Patches)
        {
            var parameterTypes = patch.ParameterTypes ?? Array.Empty<Type>();
            var methodPointer = patch.Method is ".ctor" or "ctor"
                ? Memory.GetIl2CppConstructorAddress(TargetType, parameterTypes)
                : Memory.GetIl2CppMethodAddress(TargetType, patch.Method, parameterTypes);
            var signatureAddress = Memory.FindSignatureInIl2CppMethod(methodPointer, patch.Pattern, patch.ScanSize);

            if (signatureAddress == 0)
            {
                yield return new PatchingException($"Could not find pattern for patch \"{patch.Description}\".", this);
                continue;
            }

            var patchAddress = signatureAddress + patch.Offset;

            if (PatchedAddresses.Contains(patchAddress))
            {
                var message = $"Patching the same address {patchAddress.ToString("X")}! Something is wrong!";
                L.Fatal(message);
                yield return new PatchingException(message, this);
                continue;
            }

            L.Verbose($"Applying \"{patch.Description}\" patch at {patchAddress.ToString("X")}");
            Memory.PatchMemory(patchAddress, patch.Bytes);
            PatchedAddresses.Add(patchAddress);
        }
    }

    protected abstract void SetupPatches();

    protected const byte NOP = 0x90;

    protected static byte[] GenerateNop(int count)
    {
        if (count <= 0)
            throw new ArgumentException("Count must be greater than 0.", nameof(count));

        return Enumerable.Repeat(NOP, count).ToArray();
    }

    protected record Patch
    {
        public string Method { get; init; } = "";
        public string Description { get; init; } = "";
        public string Pattern { get; init; } = "";
        public int Offset { get; init; }
        public byte[] Bytes { get; init; } = Array.Empty<byte>();
        public long? ScanSize { get; init; }
        public Type[]? ParameterTypes { get; init; } = Array.Empty<Type>();
    }
}
