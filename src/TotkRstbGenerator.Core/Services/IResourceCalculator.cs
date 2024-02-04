namespace TotkRstbGenerator.Core.Services;

public interface IResourceCalculator
{
    public static abstract uint CalculateSizeOffset(Span<byte> data);
}
