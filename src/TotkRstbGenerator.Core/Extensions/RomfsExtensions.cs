using System.Runtime.CompilerServices;

namespace TotkRstbGenerator.Core.Extensions;

public static class RomfsExtensions
{
    public static int GetVersion(this string romfsFolder, int @default = 100)
    {
        string regionLangMask = Path.Combine(romfsFolder, "System", "RegionLangMask.txt");
        if (File.Exists(regionLangMask))
        {
            string[] lines = File.ReadAllLines(regionLangMask);
            if (lines.Length >= 3 && int.TryParse(lines[2], out int value))
            {
                return value;
            }
        }

        return @default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetRsizetableFile(this string romfs)
    {
        return Path.Combine(romfs, "System", "Resource", $"ResourceSizeTable.{romfs.GetVersion()}.rsizetable.zs");
    }
}
