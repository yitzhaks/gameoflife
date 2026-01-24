# Claude Development Guide

This file provides context for Claude (AI assistant) when working on this codebase.

## Project Overview

This is a C# implementation of Conway's Game of Life with support for arbitrary board topologies (not just square grids).

## Key Documents

- **[ARCHITECTURE.md](ARCHITECTURE.md)**: Core design principles and abstractions. Read this first to understand the graph-based topology approach.
- **[TASKS.md](TASKS.md)**: Current backlog and planned work.

## Development Principles

1. **Topology is a graph**: The board is fundamentally a graph of cells and neighbor relationships. Geometry is a rendering concern.

2. **Keep abstractions minimal**: `ITopology<TCell>` only needs `Cells` and `GetNeighbors()`. Resist adding more.

3. **Immutability**: Topologies and game states should be immutable. Return new instances rather than mutating.

4. **Builders over inheritance**: Use builder classes to construct topologies, not deep inheritance hierarchies.

5. **Test at the interface level**: Tests should work against `ITopology<TCell>`, not specific implementations.

## Build & Test

```bash
dotnet build
dotnet test
```

## Project Structure

See [ARCHITECTURE.md](ARCHITECTURE.md) for the planned structure. The codebase is currently being rebuilt from scratch.
