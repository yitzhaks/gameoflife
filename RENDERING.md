# Rendering

Topology defines structure, not visual position. Displaying nodes on screen requires two additional layers: **Layout** (geometry) and **Rendering** (visuals).

## Core Principle

**Layout and rendering are separate concerns.**

- **Layout engine**: Defines mapping rules from identities to grid coordinates and builds layout snapshots.
- **Layout**: Snapshot bound to a specific topology; exposes nodes, bounds, and positions.
- **Rendering**: Converts grid coordinates to concrete visual outputs, applies visual styling, outputs to target.

```
Topology (structure)
    ↓
LayoutEngine (rules)
    ↓
Layout (geometry snapshot) — nodes, bounds, TIdentity → Point2D/Point3D
    ↓
Renderer (visuals) — cell size, shape, colors, output target
```

## Taxonomy

| Concept | Role |
|---------|------|
| LayoutEngine | Builds layout snapshots from topology (mapping rules) |
| Layout | Snapshot bound to topology; maps `TIdentity` → grid position (`Point2D` or `Point3D`) |
| Bounds | Size of the grid in layout coordinates |
| Renderer | Converts layout to concrete outputs, applies styling, outputs |
| StateStyle | Maps `TState` → visual appearance (color, character) |

## Layout

Layout engines map node identities to grid positions and produce layout snapshots bound to a topology. Positions are integers representing logical grid coordinates, not output units.

### `ILayoutEngine<TIdentity, TCoordinate>`

```csharp
public interface ILayoutEngine<TIdentity, TCoordinate>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
{
    ILayout<TIdentity, TCoordinate> CreateLayout(ITopology<TIdentity> topology);
}
```

### `ILayout<TIdentity, TCoordinate>`

```csharp
public interface ILayout<TIdentity, TCoordinate>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
{
    IEnumerable<TIdentity> Nodes { get; }
    TCoordinate GetPosition(TIdentity node);
    IBounds<TCoordinate> Bounds { get; }
}

public interface IBounds<TCoordinate> where TCoordinate : struct
{
    TCoordinate Min { get; }
    TCoordinate Max { get; }
}
```

**Design notes:**
- `TCoordinate` is `Point2D` or `Point3D` (integer-based grid positions)
- `ILayoutEngine` should be reusable and stateless
- Layouts are bound to the topology used to create them; `Nodes` is a fixed snapshot
- `GetPosition` should be pure and deterministic and throws if the node is unknown to the layout
- Layouts should not maintain mutable state

### Coordinate Types

| Type | Use Case |
|------|----------|
| `Point2D` | Standard 2D grids (rectangular, hex projected to 2D) |
| `Point3D` | 3D visualizations, layered grids |

The difference between square and hex grids is in how identities map to grid positions—the renderer decides the visual shape.

### Standard Implementations

| Implementation | Description |
|----------------|-------------|
| `IdentityLayoutEngine` | For `Point2D`/`Point3D` identities, returns identity as-is |
| `HexLayoutEngine` | Maps axial hex coordinates to staggered 2D grid positions |

For non-standard topologies, implement `ILayoutEngine<TIdentity, TCoordinate>` with custom logic.

## Rendering

Converts layout positions to visual output. Handles output-specific concerns.

### Renderer Responsibilities

- **Grid to outputs**: Cell size, spacing between cells
- **Node enumeration**: Use `layout.Nodes` as the authoritative list of nodes to render
- **Cell shape**: Square, hexagon, circle (independent of topology)
- **Cell styling**: Colors, characters, borders based on state
- **Output**: Console, image file, animation

### `IRenderer<TIdentity, TState, TCoordinate>`

```csharp
public interface IRenderer<TIdentity, TState, TCoordinate>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
{
    void Render(
        ILayout<TIdentity, TCoordinate> layout,
        IGeneration<TIdentity, TState> generation);
}
```

**Design notes:**
- **Coordinate-specific**: Renderers are bound to a coordinate type via `TCoordinate`. Current renderers target `Point2D`; 3D renderers are future work.
- **Layout is authoritative**: Renderers enumerate `layout.Nodes`; if `generation` does not know a node, renderers should treat it as an error (e.g., propagate the exception).
- `layout`: Provides grid positions and the list of nodes to render
- `generation`: Provides each node's current state

### `IStateStyle<TState, TVisual>`

Maps cell states to visual representation.

```csharp
public interface IStateStyle<TState, TVisual>
{
    TVisual GetVisual(TState state);
}
```

`TVisual` varies by renderer:

| Renderer | TVisual | Example |
|----------|---------|---------|
| Console | `ConsoleCell` | `('█', ConsoleColor.Green, ConsoleColor.Black)` |
| Image | `Color` | `Color.FromRgb(0, 255, 0)` |

### Console Rendering

```csharp
public readonly record struct ConsoleCell(
    char Character,
    ConsoleColor Foreground,
    ConsoleColor Background);

public class BoolConsoleStyle : IStateStyle<bool, ConsoleCell>
{
    public ConsoleCell Alive { get; init; } = new('█', ConsoleColor.Green, ConsoleColor.Black);
    public ConsoleCell Dead { get; init; } = new(' ', ConsoleColor.Black, ConsoleColor.Black);

    public ConsoleCell GetVisual(bool state) => state ? Alive : Dead;
}
```

**Console renderer options:**
- Cell width (characters per cell, to adjust aspect ratio)
- Border characters (optional grid lines)

### Image Rendering

```csharp
public class BoolColorStyle : IStateStyle<bool, Color>
{
    public Color Alive { get; init; } = Color.White;
    public Color Dead { get; init; } = Color.Black;

    public Color GetVisual(bool state) => state ? Alive : Dead;
}
```

**Image renderer options:**
- Cell size (output units)
- Cell shape (square, hexagon, circle)
- Cell spacing/padding
- Border width and color
- Output format (PNG, GIF)
- Animation support (multiple generations → animated GIF)

**Dependencies:**
- Image rendering uses ImageSharp (or similar)
- Kept in separate assembly to avoid core dependency

## Project Structure

*Planned structure. Rendering assemblies/APIs will be created when implementation begins. File lists are abbreviated—see interface definitions above for complete API surface.*

```
src/
├── GameOfLife.Core/              # Core library (no rendering)
├── GameOfLife.Rendering/         # Base abstractions
│   ├── ILayout.cs
│   ├── IBounds.cs
│   ├── IRenderer.cs
│   └── IStateStyle.cs
├── GameOfLife.Rendering.Console/ # Console renderer
│   ├── ConsoleRenderer.cs
│   ├── ConsoleCell.cs
│   └── BoolConsoleStyle.cs
└── GameOfLife.Rendering.Image/   # Image renderer
    ├── ImageRenderer.cs
    ├── CellShape.cs              # Square, Hexagon, Circle
    └── BoolColorStyle.cs
```

## Usage Example

```csharp
// Setup
var topology = new RectangularTopology(width: 40, height: 20);
var world = new World<Point2D, bool>(topology, new ConwayRules());
var timeline = new Timeline<Point2D, bool>(world, initialGeneration);

// Layout (identity layout for Point2D topology)
var layoutEngine = new IdentityLayoutEngine<Point2D, Point2D>();
var layout = layoutEngine.CreateLayout(topology);

// Rendering
var style = new BoolConsoleStyle();
var renderer = new ConsoleRenderer<Point2D, bool>(style, cellWidth: 2);

// Game loop
while (true)
{
    renderer.Render(layout, timeline.Current);
    timeline.Step();
    Thread.Sleep(100);
}
```

## Non-Grid Topologies

*TBD* - Rendering arbitrary graph topologies (e.g., named regions, social networks) requires layouts that assign grid positions to non-geometric identities.

**Expected approach:** Callers provide a custom `ILayoutEngine<TIdentity, Point2D>` that builds layouts from:
- External layout library (force-directed graph algorithms)
- Explicit dictionary mapping identities to positions
- Domain-specific logic (geographic coordinates)

## Design Decisions

1. **Layout is geometry, rendering is visuals**: Layout produces integer grid coordinates. Rendering handles output units, shapes, colors.

2. **Integer coordinates for layout**: Grid positions are integers. Output conversion (including fractional offsets for hex) happens in the renderer.

3. **Cell shape is a rendering choice**: The same layout can render as squares or hexagons. Topology and layout don't dictate visual shape.

4. **Rendering is pluggable**: Core library has no rendering dependencies. Renderers are separate assemblies.

5. **Style is generic**: `IStateStyle<TState, TVisual>` allows any state type to map to any visual representation.
