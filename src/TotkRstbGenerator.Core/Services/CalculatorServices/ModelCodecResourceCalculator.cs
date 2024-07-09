using Revrs.Extensions;
using System.Runtime.CompilerServices;

namespace TotkRstbGenerator.Core.Services.CalculatorServices;

public class ModelCodecResourceCalculator : IResourceCalculator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CalculateSizeOffset(Span<byte> data)
    {
        int flags = data[8..].Read<int>();
        int size = (flags >> 5) << (flags & 0xF);
        return (uint)((size * 1.15 + 0x1000) * 4);
    }
}
