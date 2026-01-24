# Architecture

## Core Principle

**Structure and state are separate concerns.**

- **Structure** (topology): What positions exist? Who neighbors whom? *Immutable.*
- **State**: What is the current state at each position? *Immutable, but replaced each generation.*

The Game of Life algorithm doesn't care about coordinates or geometry. It only needs to know:
1. What positions exist?
2. For each position, who are its neighbors?
3. What is the current state at each position?

Everything else - coordinates, shapes, rendering - is metadata for construction and display.

## Core Abstractions

### ITopology&lt;TPosition&gt;

Defines the **structure** of a board - positions and their neighbor relationships. No state, purely structural.

```csharp
public interface ITopology<TPosition> where TPosition : notnull
{
    IEnumerable<TPosition> Positions { get; }
    IEnumerable<TPosition> GetNeighbors(TPosition position);
}
```

`TPosition` can be anything: `string`, `int`, `Point2D`, a custom struct - whatever identifies a location in the topology.

### IRules&lt;TState&gt;

Defines how state evolves. Generic over `TState` to support multi-state automata.

```csharp
public interface IRules<TState>
{
    TState DefaultState { get; }
    TState GetNextState(TState current, IEnumerable<TState> neighborStates);
}
```

- `DefaultState`: What unspecified positions default to (e.g., `false`/dead for Game of Life)
- `GetNextState`: Given current state and neighbor states, compute the next state

Classic Game of Life uses `TState = bool` with B3/S23 rules.

### Game&lt;TPosition, TState&gt;

Combines topology with rules and a **sparse state map**. Each tick produces a new `Game` with updated state.

```csharp
public class Game<TPosition, TState> where TPosition : notnull
{
    public ITopology<TPosition> Topology { get; }
    public IRules<TState> Rules { get; }
    public IReadOnlyDictionary<TPosition, TState> States { get; }  // Sparse!

    public TState GetState(TPosition position)
        => States.TryGetValue(position, out var state) ? state : Rules.DefaultState;

    public Game<TPosition, TState> Tick();  // Returns NEW game with new state map
}
```

**Why sparse?** Only positions with non-default states are stored. For a large board with few alive cells, this is efficient. For dense states, it still works - just stores more entries.

## Topology Implementations

### GraphTopology&lt;TPosition&gt;

The most flexible implementation. Manually define positions and their connections.

```csharp
var topology = new GraphTopologyBuilder<string>()
    .AddPosition("A")
    .AddPosition("B")
    .AddPosition("C")
    .Connect("A", "B")
    .Connect("B", "C")
    .Build();
```

### Grid Builders

Helper classes that generate topologies with regular patterns:

- **SquareGridBuilder**: Traditional 2D grid (4 or 8 neighbors)
- **HexGridBuilder**: Hexagonal grid using cube coordinates (6 neighbors)
- **CubicGridBuilder**: 3D cubic lattice (6 or 26 neighbors)

These are conveniences - they produce standard `ITopology<T>` instances.

### CompositeTopology

Joins multiple topologies together with explicit portal connections.

```csharp
var hex = HexGridBuilder.Create(radius: 5);
var square = SquareGridBuilder.Create(10, 10);

var composite = new CompositeTopologyBuilder<Position>()
    .Add("hex", hex)
    .Add("square", square)
    .Connect(hex.GetPosition(...), square.GetPosition(...))
    .Build();
```

This enables hybrid boards: a hexagonal region connected to a square grid connected to a 3D section.

## Rendering (Separate Concern)

Topology defines structure, not visual position. Rendering requires additional metadata:

```csharp
public interface IRenderer<TPosition, TState>
{
    Vector2 GetRenderPosition(TPosition position);  // Where to draw
    Shape GetShape(TPosition position);             // What shape
    Color GetColor(TState state);                   // How to color based on state
}
```

Grid builders can provide default render info. Custom topologies supply their own.

## Project Structure

```
src/
├── Topology/                    # Core structural abstractions
│   ├── ITopology.cs
│   ├── GraphTopology.cs
│   ├── GraphTopologyBuilder.cs
│   └── CompositeTopology.cs
│
├── Topology.Grids/              # Grid builders (optional)
│   ├── SquareGridBuilder.cs
│   ├── HexGridBuilder.cs
│   └── CubicGridBuilder.cs
│
├── CellularAutomata/            # Game logic (generic over state)
│   ├── IRules.cs
│   ├── Game.cs
│   └── Rules/
│       ├── GameOfLifeRules.cs   # Classic B3/S23
│       ├── HighLifeRules.cs     # B36/S23
│       └── BriansBrainRules.cs  # 3-state example
│
└── Rendering/                   # Display (future)
    └── ...
```

## Design Decisions

1. **Structure and state are separate**: Topology defines *where* positions are and *who* neighbors whom. State defines *what* each position currently is. This separation keeps the topology immutable and reusable.

2. **Everything is immutable**: Topologies never change. State maps never change. `Tick()` returns a *new* game with a *new* state map. This enables safe sharing, caching, and easy undo/history.

3. **State is sparse**: Only positions with non-default states are stored in the state map. This is efficient for typical Game of Life patterns (mostly dead cells) while still supporting dense states.

4. **Positions are their own identity**: No separate ID system. A `Point2D` position is identified by its coordinates. A `string` position is identified by its value. Generic `TPosition` keeps it flexible.

5. **Rules are generic over state**: `IRules<TState>` supports any state type - `bool` for classic Game of Life, `enum` for multi-state automata like Brian's Brain, or custom types for more complex simulations.

6. **Builders, not inheritance**: Grid topologies are built by helper classes, not by subclassing. This avoids deep inheritance hierarchies and keeps the core interface simple.
