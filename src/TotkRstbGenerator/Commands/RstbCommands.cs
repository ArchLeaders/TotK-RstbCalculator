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

    [Command("patch", Description = "Calculate the RSTB values for a input rstbs")]
    public async Task Patch([Argument] string romfs, [Argument] string[] rstbs, [Option("output", ['o'], Description = "Output Folder")] string? output, [Option("padding", ['p'], Description = "Adds padding to every generated RSTB value")] uint padding = 0)
    {
        // Prepare Vars
        List<Task> tasks = new List<Task>();

        // Create Tasks
        foreach (var rstb in rstbs)
        {
            tasks.Add(Task.Run(async () => {
                RstbGenerator generator = new RstbGenerator(romfs, rstb, output, padding);
                await generator.GenerateAsync();
            }));
        }

        // Run Tasks
        await Task.WhenAll(tasks);
    }
}
