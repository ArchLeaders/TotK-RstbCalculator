using Cocona;
using TotkRstbGenerator.Commands;

CoconaApp app = CoconaApp.Create(args);
app.AddCommands<RstbCommands>();
await app.RunAsync();