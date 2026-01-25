# Architecture

## Core Principle

**Structure and state are separate concerns.**

- **Structure** (topology): What nodes exist? Who neighbors whom? *Immutable.*
- **State**: What is the current state at each node? *Immutable, but replaced each generation.*

The Game of Life algorithm doesn't care about coordinates or geometry. It only needs to know:
1. What nodes exist?
2. For each node, who are its neighbors?
3. What is the current state at each node?

Everything else - coordinates, shapes, rendering - is metadata for construction and display.

## Taxonomy

| Concept | Role |
|---------|------|
| Topology | Structure - nodes and neighbor relationships |
| Generation | State snapshot at a moment in time |
| Rules | Logic for computing next state |
| World | Engine - combines topology + rules, computes ticks |
| Timeline | Runner - holds world + current state, steps forward |

## Core Abstractions

### `ITopology<TIdentity>`

Defines the **structure** of a board - nodes and their neighbor relationships. No state, purely structural.

```csharp
public interface ITopology<TIdentity> where TIdentity : notnull, IEquatable<TIdentity>
{
    IEnumerable<TIdentity> Nodes { get; }
    IEnumerable<TIdentity> GetNeighbors(TIdentity node);
}
```

**Constraints:**
- `TIdentity` must implement `IEquatable<TIdentity>` for efficient lookups and hash-based storage.
- Each identity must appear exactly once. Implementations should enforce uniqueness and provide O(1) lookup.
- Neighbors must be symmetric: if A is a neighbor of B, then B must be a neighbor of A. Builders should validate this when constructing the topology.

`TIdentity` is whatever identifies a node in the topology. Examples:

| Identity Type | Use Case |
|---------------|----------|
| `Point2D` | Square or hexagonal grids with 2D coordinates |
| `Point3D` | 3D cubic lattices |
| `string` | Named regions ("Sector7", "RoomA") in Doom-like maps |
| `int` | Simple numeric IDs for arbitrary graphs |
| Custom struct | Domain-specific identifiers |

The identity **is** the node - no separate ID system needed.

### `IGeneration<TIdentity, TState>`

A snapshot of state at a moment in time. Immutable.

```csharp
public interface IGeneration<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    TState this[TIdentity node] { get; }  // Throws if node is unknown
}
```

The indexer throws if the node is unknown to the generation. How state is stored internally is an implementation detail:
- Sparse dictionary for large boards with few active nodes
- 2D array for small fixed grids
- Whatever fits the use case

### `IRules<TState>`

Defines how state evolves. Generic over `TState` to support multi-state automata.

```csharp
public interface IRules<TState>
{
    TState DefaultState { get; }
    TState GetNextState(TState current, IEnumerable<TState> neighborStates);
}
```

- `DefaultState`: What unspecified nodes default to (e.g., `false`/dead for Game of Life)
- `GetNextState`: Given current state and neighbor states, compute the next state

Classic Game of Life uses `TState = bool` with B3/S23 rules.

### `World<TIdentity, TState>`

The "engine" - combines topology with rules. Stateless. Computes next generations from input.

```csharp
public class World<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    public ITopology<TIdentity> Topology { get; }
    public IRules<TState> Rules { get; }

    public IGeneration<TIdentity, TState> Tick(IGeneration<TIdentity, TState> current);
}
```

### `Timeline<TIdentity, TState>`

Holds a `World` and current state. This is where state lives.

```csharp
public class Timeline<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    public World<TIdentity, TState> World { get; }
    public IGeneration<TIdentity, TState> Current { get; }

    public void Step();              // Advance one generation
    public void Step(int count);     // Advance multiple generations
}
```

## Rendering

See [RENDERING.md](RENDERING.md).

## Project Structure

*Planned structure. Core APIs are not yet fully implemented, and rendering projects are planned but not created. File lists are abbreviated—see RENDERING.md for complete rendering API.*

```
GameOfLife.slnx                      # Solution file
src/
├── GameOfLife.Core/                 # Core library (no rendering dependencies)
│   ├── ITopology.cs
│   ├── IGeneration.cs
│   ├── IRules.cs
│   ├── World.cs
│   └── Timeline.cs
├── GameOfLife.Rendering/            # Base rendering abstractions
│   ├── ILayout.cs
│   ├── IBounds.cs
│   ├── IRenderer.cs
│   └── IStateStyle.cs
├── GameOfLife.Rendering.Console/    # Console renderer
│   └── ConsoleRenderer.cs
└── GameOfLife.Rendering.Image/      # Image renderer (ImageSharp dependency)
    └── ImageRenderer.cs

tests/
└── GameOfLife.Core.Tests/           # Unit tests (xUnit + Coverlet)
    └── ...
```

## Design Decisions

1. **Structure and state are separate**: Topology defines *what* nodes exist and *who* neighbors whom. Generation defines *what* each node currently is. This separation keeps the topology immutable and reusable across generations.

2. **Immutability**: Topologies never change. Generations never change. `Tick()` returns a *new* generation. This enables safe sharing, caching, and simplifies reasoning.

3. **World is stateless**: World is just an engine (topology + rules). State lives in Timeline or is passed through `Tick()`. This keeps World simple and reusable.

4. **Generation storage is flexible**: Implementations can be sparse (dictionary), dense (array), or anything else. The interface only requires retrieving state by identity.

5. **Nodes are their own identity**: No separate ID system. A `Point2D` is identified by its coordinates. A `string` is identified by its value. Generic `TIdentity` keeps it flexible.

6. **Rules are generic over state**: `IRules<TState>` supports any state type - `bool` for classic Game of Life, `enum` for multi-state automata, or custom types for complex simulations.
