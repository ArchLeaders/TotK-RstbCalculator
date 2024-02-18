using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using TotkRstbGenerator.Core.Extensions;

namespace TotkRstbGenerator.Core.Models;

public class TotkConfig
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Totk", "Config.json");

    private static readonly Lazy<TotkConfig> _shared = new(Load);
    public static TotkConfig Shared => _shared.Value;

    public required string GamePath { get; set; }

    [JsonIgnore]
    public string RsizetablePath => Path.Combine(GamePath, "System", "Resource", $"ResourceSizeTable.Product.{Version}.rsizetable.zs");

    [JsonIgnore]
    public string ZsDicPath => Path.Combine(GamePath, "Pack", "ZsDic.pack.zs");

    [JsonIgnore]
    public int Version => GamePath.GetVersion();

    public static TotkConfig Load()
    {
        if (!File.Exists(_path)) {
            throw new DataException("""
                A TotK configuration could not be found.
                """);
        }

        using FileStream fs = File.OpenRead(_path);
        return JsonSerializer.Deserialize(fs, TotkConfigSerializerContext.Default.TotkConfig) ?? throw new InvalidOperationException("""
            The TotK config could not be parsed. The deserializer returned null.
            """);
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        using FileStream fs = File.Create(_path);
        JsonSerializer.Serialize(fs, this, TotkConfigSerializerContext.Default.TotkConfig);
    }
}

[JsonSerializable(typeof(TotkConfig))]
public partial class TotkConfigSerializerContext : JsonSerializerContext
{

}