# Claude Development Guide

This file provides context for Claude (AI assistant) when working on this codebase.

## Project Overview

This is a C# implementation of Conway's Game of Life with support for arbitrary board topologies (not just square grids).

## Technology Stack

- **Language**: C# (.NET 10)
- **Testing**: xUnit with Coverlet for code coverage
- **Coverage Goal**: As close to 100% branch coverage as practical

## Key Documents

- **[ARCHITECTURE.md](ARCHITECTURE.md)**: Core design principles and abstractions. Read this first.
- **[TASKS.md](TASKS.md)**: Current backlog and planned work.
- **[RENDERING.md](RENDERING.md)**: Rendering architecture and design documentation.

## Taxonomy

See the [Taxonomy section in ARCHITECTURE.md](ARCHITECTURE.md#taxonomy).

## Development Principles

1. **Structure and state are separate**: Topology defines nodes and neighbors. Generation defines state. They don't mix.

2. **Immutability**: Topologies and generations never change. `Tick()` returns a new generation.

3. **World is stateless**: World is just an engine. State lives in Timeline or flows through `Tick()`.

4. **TIdentity must be equatable**: All identity types must implement `IEquatable<TIdentity>` for O(1) lookups.

5. **Test at the interface level**: Tests should work against `ITopology<TIdentity>`, not specific implementations.

## Documentation Updates

Architecture may evolve during development, and that is expected. When it does, update both [ARCHITECTURE.md](ARCHITECTURE.md) and [RENDERING.md](RENDERING.md) to keep the design documentation current.

## Build & Test

```bash
dotnet build
dotnet test
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
reportgenerator -reports:"coverage/**/coverage.cobertura.xml" -targetdir:"coverage/report" -reporttypes:TextSummary
```

## Testing Requirements

- **Framework**: xUnit
- **Coverage Tool**: Coverlet (via `coverlet.collector` package)
- **Goal**: Near 100% branch coverage
- **Naming**: `MethodName_Scenario_ExpectedBehavior`

## Style Preferences

- **Command-line arguments**: Always use full parameter syntax (e.g., `--start-autoplay` not `-a`, `--inject` not `-i`, `--width` not `-w`)

## Project Structure

```
src/
  GameOfLife.Core/           # Core abstractions (Topology, Generation, World)
  GameOfLife.Rendering/      # Rendering interfaces
  GameOfLife.Rendering.Console/  # Console renderer implementation
  GameOfLife.Console/        # Console application
    patterns/                # Pattern files (*.txt) for --inject
tests/
  GameOfLife.Core.Tests/
  GameOfLife.Rendering.Console.Tests/
```

## Available Patterns

Patterns are in `src/GameOfLife.Console/patterns/` and loaded via `--inject <name>@<x>,<y>`:

| Category | Patterns |
|----------|----------|
| Still lifes | block, beehive, loaf, boat, tub |
| Oscillators | blinker, toad, beacon, clock, pulsar, pentadecathlon |
| Spaceships | glider, lwss, mwss, hwss |
| Methuselahs | r-pentomino, acorn, diehard |
| Complex | gosper-glider-gun, spaceship-flotilla, infinite-growth |
