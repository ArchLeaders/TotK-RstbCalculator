using Revrs.Extensions;
using System.Runtime.CompilerServices;

namespace TotkRstbGenerator.Core.Services.CalculatorServices;

public class AinbResourceCalculator : IResourceCalculator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint CalculateSizeOffset(Span<byte> data)
    {
        uint size = 392;
        int exbOffset = data[0x44..].Read<int>();

        if (exbOffset != 0) {
            int exbCountOffset = data[(exbOffset + 0x20)..].Read<int>();
            uint exbSignatureCount = data[(exbOffset + exbCountOffset)..].Read<uint>();
            size += 16 + (exbSignatureCount + 1) / 2 * 8;
        }

        return size;
    }
}
