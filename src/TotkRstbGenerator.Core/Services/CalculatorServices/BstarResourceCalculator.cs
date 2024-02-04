using Revrs.Extensions;
using System.Runtime.CompilerServices;

namespace TotkRstbGenerator.Core.Services.CalculatorServices;

public class BstarResourceCalculator : IResourceCalculator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CalculateSizeOffset(Span<byte> data)
    {
        uint entryCount = data[0x8..0xC].Read<uint>();
        return 0x120 + entryCount * 8;
    }
}
