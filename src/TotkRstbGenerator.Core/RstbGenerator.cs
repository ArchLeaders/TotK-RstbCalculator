using Revrs;
using RstbLibrary;
using RstbLibrary.Helpers;
using SarcLibrary;
using System.Buffers;
using TotkRstbGenerator.Core.Extensions;
using TotkRstbGenerator.Core.Helpers;
using TotkRstbGenerator.Core.Models;
using ZstdSharp;

namespace TotkRstbGenerator.Core;

public class RstbGenerator
{
    private readonly Rstb _vanilla;
    private readonly Rstb _result;
    private readonly string _romfs;
    private readonly string _output;
    private readonly uint _padding;

    public RstbGenerator(string romfs, string? sourceRstbPath = null, string? output = null, uint padding = 0)
    {
        string path = sourceRstbPath ?? TotkConfig.Shared.RsizetablePath;
        if (!File.Exists(path)) {
            if (sourceRstbPath == null)
            {
                throw new FileNotFoundException($"""
                The vanilla RSTB file 'System/Resource/ResourceSizeTable.Product.{TotkConfig.Shared.Version}.rsizetable.zs' could not be found in your game dump.
                """);
            }
            else
            {
                throw new FileNotFoundException($"""
                Input path: {sourceRstbPath} is not a valid RSTB file.
                """);
            }
        }

        _romfs = romfs;
        _output = output != null ? Path.Combine(output, Path.GetFileName(path)) : romfs.GetRsizetableFile();
        _padding = padding;

        using FileStream fs = File.OpenRead(path);
        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fs.Length);
        int size = fs.Read(buffer);

        Span<byte> data = ZstdExtension.Decompress(buffer.AsSpan()[..size]);

        _vanilla = Rstb.FromBinary(data);
        _result = Rstb.FromBinary(data);

        ArrayPool<byte>.Shared.Return(buffer);
    }

    public async Task GenerateAsync()
    {
        await GenerateAsync(_romfs);

        if (Path.GetDirectoryName(_output) is string path) {
            Directory.CreateDirectory(path);
        }

        byte[] data = _result.ToBinary();
        using FileStream fs = File.Create(_output);
        Compressor compressor = new(17);
        fs.Write(compressor.Wrap(data));
    }

    private async Task GenerateAsync(string src)
    {
        Task[] tasks = [
            Task.Run(() => Parallel.ForEachAsync(Directory.EnumerateDirectories(src), async (folder, token) => {
                await GenerateAsync(folder);
            })),
            Task.Run(() => Parallel.ForEachAsync(Directory.EnumerateFiles(src), (file, token) => {
                InjectFile(file);
                return ValueTask.CompletedTask;
            }))
        ];

        await Task.WhenAll(tasks);
    }

    private void InjectFile(string file)
    {
        string extension = file.GetRomfsExtension(out bool isZsCompressed);
        string canonical = Path.GetRelativePath(_romfs, file).ToCanonical();

        if (canonical is "Pack/ZsDic.pack" || extension is ".rsizetable" or ".bwav" or ".webm") {
            return;
        }

        if (extension is ".pack") {
            InjectPackFile(file, isZsCompressed);
        }

        if (extension is ".asb" or ".ainb" or ".bstar" or ".mc") {
            using FileStream fs = File.OpenRead(file);
            int size = (int)fs.Length;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
            fs.Read(buffer, 0, size);

            Span<byte> data = buffer.AsSpan()[..size];
            if (isZsCompressed) {
                data = data.Decompress();
            }

            InjectFile(canonical, extension, (uint)data.Length, data);

            ArrayPool<byte>.Shared.Return(buffer);
            return;
        }

        InjectFile(canonical, extension,
            size: isZsCompressed ? ZstdExtension.GetDecompressedSize(file) : (uint)new FileInfo(file).Length,
            data: []);
    }

    private void InjectFile(string canonical, string extension, uint size, Span<byte> data)
    {
        size += size.AlignUp(0x20U);
        size = ResourceSizeHelper.EstimateSize(size, canonical, extension, data);
        size += _padding;

        lock (_result)
        {
            if (_result.OverflowTable.ContainsKey(canonical))
            {
                _result.OverflowTable[canonical] = size;
                return;
            }

            uint hash = Crc32.Compute(canonical);
            if (_result.HashTable.ContainsKey(hash))
            {
                // If the hash is not in the vanilla
                // RSTB it is a hash collision
                if (!_vanilla.HashTable.ContainsKey(hash))
                {
                    _result.OverflowTable[canonical] = size;
                    return;
                }
            }

            _result.HashTable[hash] = size;
        }
    }

    private void InjectPackFile(string file, bool isZsCompressed)
    {
        using FileStream fs = File.OpenRead(file);
        int size = (int)fs.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(size);
        fs.Read(buffer, 0, size);

        Span<byte> sarcData = buffer.AsSpan()[..size];
        if (isZsCompressed) {
            sarcData = ZstdExtension.Decompress(sarcData);
        }

        RevrsReader reader = new(sarcData);
        ImmutableSarc sarc = new(ref reader);
        foreach ((var path, var inlineData) in sarc) {
            string extension = path.GetRomfsExtension(out isZsCompressed);

            Span<byte> data = inlineData;
            if (isZsCompressed) {
                data = inlineData.Decompress();
            }

            InjectFile(path.ToCanonical(), extension, (uint)data.Length, data);
        }

        ArrayPool<byte>.Shared.Return(buffer);
    }
}
