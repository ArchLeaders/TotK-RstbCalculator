# TotK RSTB/RESTBL Generator

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/ArchLeaders/TotkRstbGenerator/blob/master/License.md) [![Downloads](https://img.shields.io/github/downloads/ArchLeaders/TotkRstbGenerator/total)](https://github.com/ArchLeaders/TotkRstbGenerator/releases)

RSTB/RESTBL generator for TotK, based on [restbl](https://github.com/MasterBubbles/restbl) by [dt-12345](https://github.com/dt-12345) and [Lord Bubbles](https://github.com/MasterBubbles).

Primarily created for [TKMM](https://github.com/TKMM-Team/Tkmm)... ~~and I was bored~~.

## CLI Usage

```
...
```

## API Usage

```cs
// Single-threaded
RstbGenerator.Generate(romfs: "path/to/mod/romfs");

// Multi-threaded
await RstbGenerator.GenerateAsync(romfs: "path/to/mod/romfs");
```

## Install

[![NuGet](https://img.shields.io/nuget/v/TotkRstbGenerator.svg)](https://www.nuget.org/packages/TotkRstbGenerator) [![NuGet](https://img.shields.io/nuget/dt/TotkRstbGenerator.svg)](https://www.nuget.org/packages/TotkRstbGenerator)

#### NuGet
```powershell
Install-Package TotkRstbGenerator.Core
```

#### Build From Source
```batch
git clone https://github.com/ArchLeaders/TotkRstbGenerator.git
dotnet build src/TotkRstbGenerator
```