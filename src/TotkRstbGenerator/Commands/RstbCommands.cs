using Cocona;
using TotkRstbGenerator.Core;

namespace TotkRstbGenerator.Commands;

public class RstbCommands
{
    private const string GEN_OUTPUT_DESC = """
        The absolue path to an output RESTBL file.
        
        By default, the output is placed in the target RomFS folder canonically.
        """;

    private const string GEN_SINGLE_DESC = """
        The absolue path to an output RESTBL file.
        
        By default, the output is placed in the target RomFS folder canonically.
        """;

    [Command("generate", Aliases = ["gen", "calculate", "calc"], Description = "Calculate the RSTB values for a target RomFS folder")]
    public async Task Generate(string romfs,
        [Option("output", ['o'], Description = GEN_OUTPUT_DESC)] string? output,
        [Option("single", ['s'], Description = GEN_SINGLE_DESC)] bool single = false)
    {
        if (single == true)
        {
            RstbGenerator.Generate(romfs, output);
        }
        else
        {
            await RstbGenerator.GenerateAsync(romfs, output);
        }
    }
}
