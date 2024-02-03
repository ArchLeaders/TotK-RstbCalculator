using RstbLibrary;
using System.Buffers;
using System.Runtime.CompilerServices;
using TotkRstbGenerator.Core.Extensions;
using TotkRstbGenerator.Core.Models;
using ZstdSharp;

namespace TotkRstbGenerator.Core;

public class RstbGenerator(string romfs, string? output)
{
    private readonly Rstb _rstb = ReadVanillaRsizetableFile();
    private readonly string _romfs = romfs;
    private readonly string _output = output ?? romfs.GetRsizetableFile();

    public async Task GenerateAsync()
    {
        await GenerateAsync(_romfs);

        if (Path.GetDirectoryName(_output) is string path) {
            Directory.CreateDirectory(path);
        }

        byte[] data = _rstb.ToBinary();
        using FileStream fs = File.Create(_output);
        Compressor compressor = new();
        fs.Write(compressor.Wrap(data));
    }

    private async Task GenerateAsync(string src)
    {
        Task[] tasks = [
            Task.Run(() => Parallel.ForEachAsync(Directory.EnumerateDirectories(src), async (folder, token) => {
                await GenerateAsync(folder);
            })),
            Task.Run(() => Parallel.ForEachAsync(Directory.EnumerateFiles(src), (file, token) => {
                _rstb.InjectFileCalculation(_romfs, file);
                return ValueTask.CompletedTask;
            }))
        ];

        await Task.WhenAll(tasks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Rstb ReadVanillaRsizetableFile()
    {
        string path = TotkConfig.Shared.RsizetablePath;
        if (!File.Exists(path)) {
            throw new FileNotFoundException($"""
                The vanilla RSTB file 'System/Resource/ResourceSizeTable.Product.{TotkConfig.Shared.Version}.rsizetable.zs' could not be found in your game dump.
                """);
        }

        using FileStream fs = File.OpenRead(path);
        byte[] buffer = ArrayPool<byte>.Shared.Rent((int)fs.Length);
        int size = fs.Read(buffer);

        Span<byte> data = ZstdExtension.Decompress(buffer.AsSpan()[..size]);

        Rstb rstb = Rstb.FromBinary(data);
        ArrayPool<byte>.Shared.Return(buffer);

        return rstb;
    }
}
