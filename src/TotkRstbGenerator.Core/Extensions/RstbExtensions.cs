using Revrs;
using Revrs.Extensions;
using RstbLibrary;
using RstbLibrary.Helpers;
using System.Buffers;
using TotkRstbGenerator.Core.Models;

namespace TotkRstbGenerator.Core.Extensions;

public static class RstbExtensions
{
    public static void InjectFileCalculation(this Rstb rstb, string romfs, string file)
    {
        if (file.EndsWith("ZsDic.pack.zs")) {
            return;
        }

        int size = Calculate(file, romfs, out string? canonical, out string? relativePath);
        if (canonical is not null && relativePath is not null) {
            InjectFileCalculation(rstb, canonical, relativePath, (uint)size);
        }
    }

    public static void InjectFileCalculation(this Rstb rstb, string canonical, string relativePath, uint size)
    {
        if (rstb.OverflowTable.ContainsKey(canonical)) {
            rstb.OverflowTable[canonical] = size;
            return;
        }

        uint hash = Crc32.Compute(canonical);
        if (rstb.HashTable.ContainsKey(hash)) {
            string vanillaPath = Path.Combine(TotkConfig.Shared.GamePath, relativePath);

            if (!File.Exists(vanillaPath)) {
                rstb.OverflowTable[canonical] = size;
                return;
            }
        }

        rstb.HashTable[hash] = size;
    }

    public static int Calculate(this string file, string romfs, out string? canonical, out string? relativePath)
    {
        string ext = file.GetRomfsExtension(out bool isZsCompressed);
        if (ext is ".rsizetable" or ".bwav" or ".webm" or ".ta") {
            relativePath = null;
            canonical = null;
            return -1;
        }

        relativePath = Path.GetRelativePath(romfs, file);
        canonical = relativePath.ToCanonical();

        return ext switch {
            ".asb" or ".ainb" or ".bstar" or ".mc" => CalculateData(file, ext, isZsCompressed),
            _ => CalculateFile(file, canonical, ext, isZsCompressed)
        };
    }

    private static int CalculateFile(this string file, string canonical, string ext, bool isZsCompressed)
    {
        int size = (int)new FileInfo(file).Length;
        if (isZsCompressed) {
            size = file.GetDecompressedSize();
        }

        size += size.AlignUp(0x20);

        if (ext == ".bgyml") {
            return (size + 1000) * 8;
        }

        if (RstbInfo.ExceptionSizeOffsetMap.TryGetValue(canonical, out int offset)) {
            return size + offset;
        }

        if (RstbInfo.SizeOffsetMap.TryGetValue(ext, out offset)) {
            return size + offset;
        }

        return (size + 1500) * 4;
    }

    private static int CalculateData(this string file, string ext, bool isZsCompressed)
    {
        FileInfo info = new(file);
        int size = (int)info.Length;
        byte[] rented = ArrayPool<byte>.Shared.Rent(size);
        Span<byte> buffer = rented.AsSpan()[..size];

        using (FileStream fs = info.OpenRead()) {
            fs.Read(buffer);
        };

        if (isZsCompressed) {
            buffer = buffer.Decompress();
        }

        switch (ext) {
            case ".ainb": {
                size += 392;
                int exbOffset = buffer[0x44..].Read<int>();

                if (exbOffset != 0) {
                    int exbCountOffset = buffer[(exbOffset + 0x20)..].Read<int>();
                    int exbSignatureCount = buffer[(exbOffset + exbCountOffset)..].Read<int>();
                    size += 16 + (exbSignatureCount + 1) / 2 * 8;
                }

                break;
            }
            case ".asb": {
                int nodeCount = buffer[0x10..0x14].Read<int>();
                int exbOffset = buffer[0x60..0x64].Read<int>();
                size += 552 + 40 * nodeCount;

                if (exbOffset != 0) {
                    int exbCountOffset = buffer[(exbOffset + 0x20)..].Read<int>();
                    int exbSignatureCount = buffer[(exbOffset + exbCountOffset)..].Read<int>();
                    size += 16 + (exbSignatureCount + 1) / 2 * 8;
                }

                break;
            }
            case "bstar": {
                int entryCount = buffer[0x8..0xC].Read<int>();
                size += entryCount * 8;
                break;
            }
            case ".mc": {
                int flags = buffer[8..].Read<int>();
                size = (flags >> 5) << (flags & 0xF);
                size = (int)Math.Round((size * 1.05 + 4096) * 3.6);
                break;
            }
        }

        ArrayPool<byte>.Shared.Return(rented);
        return size;
    }
}
