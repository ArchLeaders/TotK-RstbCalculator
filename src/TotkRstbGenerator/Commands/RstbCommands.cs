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
    public async Task Generate([Argument] string romfs, [Option("rstbs", ['r'], Description = "An list of rstb files to use to patch.")] string[]? rstbs, [Option("output", ['o'], Description = "Output Folder")] string? output, [Option("padding", ['p'], Description = "Adds padding to every generated RSTB value")] uint padding = 0)
    {
        // Calculate list of rstbs
        if (rstbs != null)
        {
            List<Task> tasks = new List<Task>();

            foreach (var rstb in rstbs)
            {
                tasks.Add(Task.Run(async () => {
                    RstbGenerator generator = new RstbGenerator(romfs, rstb, output, padding);
                    await generator.GenerateAsync();
                }));
            }

            await Task.WhenAll(tasks);
            return;
        }

        // Regular run
        RstbGenerator generator = new(romfs, null, output, padding);
        await generator.GenerateAsync();
    }
}
