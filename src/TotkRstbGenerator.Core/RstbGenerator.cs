using Revrs;
using RstbLibrary;
using RstbLibrary.Helpers;
using SarcLibrary;
using System.Buffers;
using TotkCommon;
using TotkRstbGenerator.Core.Extensions;
using TotkRstbGenerator.Core.Helpers;
using ZstdSharp;

namespace TotkRstbGenerator.Core;

public class RstbGenerator
{
    private readonly Rstb _vanilla;
    private readonly Rstb _result;
    private readonly string _romfs;
    private readonly string _output;
    private readonly uint _padding;

    public RstbGenerator(string romfs, string? output = null, uint padding = 0)
    {
        _romfs = romfs;
        _output = output ?? romfs.GetRsizetableFile();
        _padding = padding;

        string path = Path.Combine(Totk.Config.GamePath, "System", "Resource", $"ResourceSizeTable.Product.{Totk.Config.Version}.rsizetable.zs");
        if (!File.Exists(path)) {
            throw new FileNotFoundException($"""
                The vanilla RSTB file 'System/Resource/ResourceSizeTable.Product.{Totk.Config.Version}.rsizetable.zs' could not be found in your game dump.
                """);
        }

        using FileStream fs = File.OpenRead(path);
        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fs.Length);
        int size = fs.Read(buffer);

        Span<byte> data = Totk.Zstd.Decompress(buffer.AsSpan()[..size]);

        _vanilla = Rstb.FromBinary(data);
        _result = Rstb.FromBinary(data);

        ArrayPool<byte>.Shared.Return(buffer);
    }

    public RstbGenerator(string romfs, string sourceRstbPath, string? outputFolder = null, uint padding = 0)
    {
        if (!File.Exists(sourceRstbPath)) {
            throw new FileNotFoundException($"The file '{sourceRstbPath}' could not be found.");
        }

        _romfs = romfs;
        _output = outputFolder is not null
            ? Path.Combine(outputFolder, Path.GetFileName(sourceRstbPath))
            : Path.Combine(romfs.GetRsizetableFile(), Path.GetFileName(sourceRstbPath));
        _padding = padding;

        using FileStream fs = File.OpenRead(sourceRstbPath);
        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fs.Length);
        int size = fs.Read(buffer);

        Span<byte> data = Totk.Zstd.Decompress(buffer.AsSpan()[..size]);

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
        await using FileStream fs = File.Create(_output);
        Compressor compressor = new(17);
        fs.Write(compressor.Wrap(data));
    }

    private async Task GenerateAsync(string src)
    {
        Task[] tasks = [
            Task.Run(() => Parallel.ForEachAsync(Directory.EnumerateDirectories(src), async (folder, _) => {
                await GenerateAsync(folder);
            })),
            Task.Run(() => Parallel.ForEachAsync(Directory.EnumerateFiles(src), (file, _) => {
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
                data = Totk.Zstd.Decompress(data);
            }

            InjectFile(canonical, extension, (uint)data.Length, data);

            ArrayPool<byte>.Shared.Return(buffer);
            return;
        }

        FileInfo fileInfo = new(file);

        if (isZsCompressed) {
            using FileStream fs = fileInfo.OpenRead();
            InjectFile(canonical, extension, size: (uint)Zstd.GetDecompressedSize(fs), data: []);
            return;
        }

        InjectFile(canonical, extension, size: (uint)fileInfo.Length, data: []);
    }

    private void InjectFile(string canonical, string extension, uint size, Span<byte> data)
    {
        size += size.AlignUp(0x20U);
        size = ResourceSizeHelper.EstimateSize(size, canonical, extension, data);
        size += _padding;

        if (_result.OverflowTable.ContainsKey(canonical)) {
            lock (_result) {
                _result.OverflowTable[canonical] = size;
            }

            return;
        }

        uint hash = Crc32.Compute(canonical);
        if (_result.HashTable.ContainsKey(hash)) {
            // If the hash is not in the vanilla
            // RSTB it is a hash collision
            if (!_vanilla.HashTable.ContainsKey(hash)) {
                lock (_result) {
                    _result.OverflowTable[canonical] = size;
                }

                return;
            }
        }

        lock (_result) {
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
            sarcData = Totk.Zstd.Decompress(sarcData);
        }

        RevrsReader reader = new(sarcData);
        ImmutableSarc sarc = new(ref reader);
        foreach ((var path, var inlineData) in sarc) {
            string extension = path.GetRomfsExtension(out isZsCompressed);

            Span<byte> data = inlineData;
            if (isZsCompressed) {
                data = Totk.Zstd.Decompress(inlineData);
            }

            InjectFile(path.ToCanonical(), extension, (uint)data.Length, data);
        }

        ArrayPool<byte>.Shared.Return(buffer);
    }
}
