# Architecture

## Core Principle

**A board is a graph.** Cells are nodes, neighbor relationships are edges.

The Game of Life algorithm doesn't care about coordinates or geometry. It only needs to know:
1. What cells exist?
2. For each cell, who are its neighbors?

Everything else - coordinates, positions, shapes - is metadata for rendering and construction.

## Core Abstractions

### ITopology<TCell>

The fundamental interface. Defines the structure of a board without any game state.

```csharp
public interface ITopology<TCell> where TCell : notnull
{
    IEnumerable<TCell> Cells { get; }
    IEnumerable<TCell> GetNeighbors(TCell cell);
}
```

`TCell` can be anything: `string`, `int`, `Point2D`, a custom struct - whatever makes sense for the topology. The cell **is** its own identity.

### IRules

Defines the birth/survival rules for the cellular automaton.

```csharp
public interface IRules
{
    bool ShouldBeAlive(bool currentlyAlive, int aliveNeighborCount);
}
```

Classic Game of Life is B3/S23: birth with 3 neighbors, survive with 2 or 3.

### Game<TCell>

Combines a topology with rules and manages cell state over generations.

```csharp
public class Game<TCell> where TCell : notnull
{
    public ITopology<TCell> Topology { get; }
    public IRules Rules { get; }
    public IReadOnlySet<TCell> AliveCells { get; }

    public Game<TCell> Tick(); // Returns next generation
}
```

## Topology Implementations

### GraphTopology<TCell>

The most flexible implementation. Manually define cells and their connections.

```csharp
var topology = new GraphTopologyBuilder<string>()
    .AddCell("A")
    .AddCell("B")
    .AddCell("C")
    .Connect("A", "B")
    .Connect("B", "C")
    .Build();
```

### Grid Builders

Helper classes that generate `GraphTopology` instances with regular patterns:

- **SquareGridBuilder**: Traditional 2D grid (4 or 8 neighbors)
- **HexGridBuilder**: Hexagonal grid using cube coordinates (6 neighbors)
- **CubicGridBuilder**: 3D cubic lattice (6 or 26 neighbors)

These are conveniences - they produce standard `ITopology<T>` instances.

### CompositeTopology

Joins multiple topologies together with explicit portal connections.

```csharp
var hex = HexGridBuilder.Create(radius: 5);
var square = SquareGridBuilder.Create(10, 10);

var composite = new CompositeTopologyBuilder<Cell>()
    .Add("hex", hex)
    .Add("square", square)
    .Connect(hex.GetCell(...), square.GetCell(...))
    .Build();
```

This enables hybrid boards: a hexagonal region connected to a square grid connected to a 3D section.

## Rendering (Separate Concern)

Topology defines structure, not position. Rendering requires additional metadata:

```csharp
public interface IRenderable<TCell>
{
    Vector2 GetRenderPosition(TCell cell);  // Where to draw
    Shape GetCellShape(TCell cell);         // How to draw
}
```

Grid builders can provide default render info. Custom topologies supply their own.

## Project Structure

```
src/
├── Topology/                    # Core abstractions
│   ├── ITopology.cs
│   ├── GraphTopology.cs
│   └── CompositeTopology.cs
│
├── Topology.Grids/              # Grid builders (optional)
│   ├── SquareGridBuilder.cs
│   ├── HexGridBuilder.cs
│   └── CubicGridBuilder.cs
│
├── GameOfLife/                  # Game logic
│   ├── IRules.cs
│   ├── ClassicRules.cs
│   └── Game.cs
│
└── GameOfLife.Rendering/        # Display (future)
    └── ...
```

## Design Decisions

1. **Topology is immutable**: Once built, the structure doesn't change. This enables safe sharing and caching.

2. **State lives in Game, not Topology**: The topology defines structure; the game tracks which cells are alive. This separates concerns cleanly.

3. **Cells are their own identity**: No separate ID system. A `Point2D` cell is identified by its coordinates. A `string` cell is identified by its value. Generic `TCell` keeps it flexible.

4. **Builders, not inheritance**: Grid topologies are built by helper classes, not by subclassing. This avoids deep inheritance hierarchies and keeps the core interface simple.

5. **Rules are pluggable**: Different cellular automata (HighLife, Day & Night, Seeds, etc.) just implement `IRules`.
