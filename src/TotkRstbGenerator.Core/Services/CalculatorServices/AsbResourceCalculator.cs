
using Revrs.Extensions;
using System.Runtime.CompilerServices;

namespace TotkRstbGenerator.Core.Services.CalculatorServices;

public class AsbResourceCalculator : IResourceCalculator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CalculateSizeOffset(Span<byte> data)
    {
        uint nodeCount = data[0x10..0x14].Read<uint>();
        int exbOffset = data[0x60..0x64].Read<int>();
        uint size = 552 + 40 * nodeCount;

        if (exbOffset != 0) {
            int exbCountOffset = data[(exbOffset + 0x20)..].Read<int>();
            uint exbSignatureCount = data[(exbOffset + exbCountOffset)..].Read<uint>();
            size += 16 + (exbSignatureCount + 1) / 2 * 8;
        }

        return size;
    }
}
