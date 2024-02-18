using Revrs;
using Revrs.Extensions;
using SarcLibrary;
using System.Buffers;
using TotkRstbGenerator.Core.Models;
using ZstdSharp;

namespace TotkRstbGenerator.Core.Extensions;

public static class ZstdExtension
{
    private const uint ZSTD_MAGIC = 0xFD2FB528;

    private static readonly Decompressor _defaultDecompressor = new();
    private static readonly Dictionary<int, byte[]> _dicts = [];

    static ZstdExtension()
    {
        if (!File.Exists(TotkConfig.Shared.ZsDicPath)) {
            throw new FileNotFoundException($"""
                The vanilla ZsDic file 'Pack/ZsDic.pack.zs' could not be found in your game dump.
                """);
        }

        using FileStream fs = File.OpenRead(TotkConfig.Shared.ZsDicPath);
        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fs.Length);
        int size = fs.Read(buffer);

        Span<byte> data = _defaultDecompressor.Unwrap(buffer.AsSpan()[..size]);
        RevrsReader reader = new(data);
        ImmutableSarc sarc = new(ref reader);

        foreach ((var _, var fileData) in sarc) {
            Decompressor decompressor = new();
            decompressor.LoadDictionary(fileData);
            int id = fileData[4..8].Read<int>();
            _dicts[id] = fileData.ToArray();
        }
    }

    public static Span<byte> Decompress(this Span<byte> buffer)
    {
        lock (_defaultDecompressor)
        {
            if (buffer.Length < 5 || buffer.Read<uint>() != ZSTD_MAGIC)
            {
                return buffer;
            }

            int id = GetDictionaryId(buffer);
            if (id > -1 && _dicts.TryGetValue(id, out byte[]? dict))
            {
                Decompressor decompressor = new();
                decompressor.LoadDictionary(dict);
                return decompressor.Unwrap(buffer);
            }

            return _defaultDecompressor.Unwrap(buffer);
        }
    }

    public static uint GetDecompressedSize(this string file)
    {
        using FileStream fs = File.OpenRead(file);
        Span<byte> header = stackalloc byte[14];
        fs.Read(header);
        return GetFrameContentSize(header);
    }

    private static uint GetFrameContentSize(Span<byte> buffer)
    {
        byte descriptor = buffer[4];
        int windowDescriptorSize = ((descriptor & 0b00100000) >> 5) ^ 0b1;
        int dictionaryIdFlag = descriptor & 0b00000011;
        int frameContentFlag = descriptor >> 6;

        int offset = dictionaryIdFlag switch {
            0x0 => 5 + windowDescriptorSize,
            0x1 => 5 + windowDescriptorSize + 1,
            0x2 => 5 + windowDescriptorSize + 2,
            0x3 => 5 + windowDescriptorSize + 4,
            _ => throw new OverflowException("""
                Two bits cannot exceed 0x3, something terrible has happened!
                """)
        };

        return frameContentFlag switch {
            0x0 => buffer[offset],
            0x1 => buffer[offset..].Read<ushort>() + 0x100U,
            0x2 => buffer[offset..].Read<uint>(),
            _ => throw new NotSupportedException("""
                64-bit file sizes are not supported.
                """)
        };
    }

    private static int GetDictionaryId(Span<byte> buffer)
    {
        byte descriptor = buffer[4];
        int windowDescriptorSize = ((descriptor & 0b00100000) >> 5) ^ 0b1;
        int dictionaryIdFlag = descriptor & 0b00000011;

        return dictionaryIdFlag switch {
            0x0 => -1,
            0x1 => buffer[5 + windowDescriptorSize],
            0x2 => buffer[(5 + windowDescriptorSize)..].Read<short>(),
            0x3 => buffer[(5 + windowDescriptorSize)..].Read<int>(),
            _ => throw new OverflowException("""
                Two bits cannot exceed 0x3, something terrible has happened!
                """)
        };
    }
}