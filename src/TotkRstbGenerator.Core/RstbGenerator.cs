using TotkRstbGenerator.Core.Extensions;

namespace TotkRstbGenerator.Core;

public class RstbGenerator
{
    public static void Generate(string romfs, string? output)
    {
        output ??= romfs.GetRsizetableFile();

        throw new NotImplementedException();
    }

    public static Task GenerateAsync(string romfs, string? output)
    {
        output ??= romfs.GetRsizetableFile();

        throw new NotImplementedException();
    }
}
