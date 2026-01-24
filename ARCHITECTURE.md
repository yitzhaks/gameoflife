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

## Core Abstractions

### ITopology&lt;TIdentity&gt;

Defines the **structure** of a board - nodes and their neighbor relationships. No state, purely structural.

```csharp
public interface ITopology<TIdentity> where TIdentity : notnull
{
    IEnumerable<TIdentity> Nodes { get; }
    IEnumerable<TIdentity> GetNeighbors(TIdentity node);
}
```

`TIdentity` is whatever identifies a node in the topology. Examples:

| Identity Type | Use Case |
|---------------|----------|
| `Point2D` | Square or hexagonal grids with 2D coordinates |
| `Point3D` | 3D cubic lattices |
| `string` | Named regions ("Sector7", "RoomA") in Doom-like maps |
| `int` | Simple numeric IDs for arbitrary graphs |
| Custom struct | Domain-specific identifiers |

The identity **is** the node - no separate ID system needed.

### Generation&lt;TIdentity, TState&gt;

A snapshot of state at a moment in time. Immutable.

```csharp
public interface IGeneration<TIdentity, TState> where TIdentity : notnull
{
    TState this[TIdentity node] { get; }
}
```

That's it. How state is stored internally is an implementation detail:
- Sparse dictionary for large boards with few active nodes
- 2D array for small fixed grids
- Whatever fits the use case

### IRules&lt;TState&gt;

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

### World&lt;TIdentity, TState&gt;

The "engine" - combines topology with rules. Stateless. Computes next generations from input.

```csharp
public class World<TIdentity, TState> where TIdentity : notnull
{
    public ITopology<TIdentity> Topology { get; }
    public IRules<TState> Rules { get; }

    public IGeneration<TIdentity, TState> Tick(IGeneration<TIdentity, TState> current);
}
```

### Timeline&lt;TIdentity, TState&gt;

Holds a `World` and current state. This is where state lives.

```csharp
public class Timeline<TIdentity, TState> where TIdentity : notnull
{
    public World<TIdentity, TState> World { get; }
    public IGeneration<TIdentity, TState> Current { get; }

    public void Step();              // Advance one generation
    public void Step(int count);     // Advance multiple generations
}
```

## Topology Implementations

### GraphTopology&lt;TIdentity&gt;

The most flexible implementation. Manually define nodes and their connections.

```csharp
var topology = new GraphTopologyBuilder<string>()
    .AddNode("A")
    .AddNode("B")
    .AddNode("C")
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

var composite = new CompositeTopologyBuilder<INodeIdentity>()
    .Add("hex", hex)
    .Add("square", square)
    .Connect(hex.GetNode(...), square.GetNode(...))
    .Build();
```

This enables hybrid boards: a hexagonal region connected to a square grid connected to a 3D section.

## Rendering (Separate Concern)

Topology defines structure, not visual position. Rendering requires additional metadata:

```csharp
public interface IRenderer<TIdentity, TState>
{
    Vector2 GetRenderPosition(TIdentity node);  // Where to draw
    Shape GetShape(TIdentity node);             // What shape
    Color GetColor(TState state);               // How to color based on state
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
├── CellularAutomata/            # Core logic (generic over state)
│   ├── Generation.cs
│   ├── IRules.cs
│   ├── World.cs
│   ├── Timeline.cs
│   └── Rules/
│       ├── GameOfLifeRules.cs   # Classic B3/S23
│       ├── HighLifeRules.cs     # B36/S23
│       └── BriansBrainRules.cs  # 3-state example
│
└── Rendering/                   # Display (future)
    └── ...
```

## Design Decisions

1. **Structure and state are separate**: Topology defines *what* nodes exist and *who* neighbors whom. State defines *what* each node currently is. This separation keeps the topology immutable and reusable.

2. **Everything is immutable**: Topologies never change. State maps never change. `Tick()` returns a *new* game with a *new* state map. This enables safe sharing, caching, and easy undo/history.

3. **Generation storage is flexible**: Implementations can be sparse (dictionary), dense (array), or anything else. The interface only requires retrieving state by identity.

4. **Nodes are their own identity**: No separate ID system. A `Point2D` node is identified by its coordinates. A `string` node is identified by its value. Generic `TIdentity` keeps it flexible.

5. **Rules are generic over state**: `IRules<TState>` supports any state type - `bool` for classic Game of Life, `enum` for multi-state automata like Brian's Brain, or custom types for more complex simulations.

6. **Builders, not inheritance**: Grid topologies are built by helper classes, not by subclassing. This avoids deep inheritance hierarchies and keeps the core interface simple.
