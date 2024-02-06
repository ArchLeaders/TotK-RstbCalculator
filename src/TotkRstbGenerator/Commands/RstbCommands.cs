using Cocona;
using TotkRstbGenerator.Core;

namespace TotkRstbGenerator.Commands;

public class RstbCommands
{
    private const string GEN_OUTPUT_DESC = """
        The absolue path to an output RESTBL file.
        
        By default, the output is placed in the target RomFS folder canonically.
        """;

    [Command("generate", Aliases = ["gen", "calculate", "calc"], Description = "Calculate the RSTB values for a target RomFS folder")]
    public async Task Generate([Argument] string romfs, [Option("output", ['o'], Description = GEN_OUTPUT_DESC)] string? output, [Option("padding", ['p'], Description = "Adds padding to every generated RSTB value")] uint padding = 0)
    {
        RstbGenerator generator = new(romfs, output, padding);
        await generator.GenerateAsync();
    }
}
