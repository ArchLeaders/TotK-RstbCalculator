using System.Runtime.CompilerServices;
using TotkRstbGenerator.Core.Models;

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
    public static string ToCanonical(this string path)
    {
        string ext = Path.GetExtension(path);
        if (ext is ".zs" or ".mc" && !path.EndsWith(".ta.zs")) {
            return path[..^3]
                .Replace(Path.DirectorySeparatorChar, '/');
        }

        return path
            .Replace(Path.DirectorySeparatorChar, '/');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetRomfsExtension(this string path, out bool isZsCompressed)
    {
        string ext = Path.GetExtension(path);
        if (ext is ".zs" && !path.EndsWith(".ta.zs")) {
            isZsCompressed = true;
            return path.EndsWith(".casset.byml.zs")
                ? ".casset.byml"
                : Path.GetExtension(path[..^3]);
        }

        isZsCompressed = false;
        return ext;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetRsizetableFile(this string romfs)
    {
        return Path.Combine(romfs, "System", "Resource", $"ResourceSizeTable.Product.{TotkConfig.Shared.Version}.rsizetable.zs");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetRsizetableFolder(this string romfs)
    {
        return Path.Combine(romfs, "System", "Resource");
    }
}
