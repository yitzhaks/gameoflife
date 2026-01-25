# Rendering

Topology defines structure, not visual position. Displaying nodes on screen requires two additional layers: **Layout** (geometry) and **Rendering** (visuals).

## Core Principle

**Layout and rendering are separate concerns.**

- **Layout**: Maps abstract node identities to grid coordinates. Pure geometry.
- **Rendering**: Converts grid coordinates to pixels, applies visual styling, outputs to target.

```
Topology (structure)
    ↓
Layout (geometry) — TIdentity → Point2D/Point3D, bounds
    ↓
Renderer (visuals) — cell size, shape, colors, output target
```

## Taxonomy

| Concept | Role |
|---------|------|
| Layout | Maps `TIdentity` → grid position (`Point2D` or `Point3D`) |
| Bounds | Size of the grid in layout coordinates |
| Renderer | Converts layout to pixels, applies styling, outputs |
| CellStyle | Maps `TState` → visual appearance (color, character) |

## Layout

Maps node identities to grid positions. Positions are integers representing logical grid coordinates, not pixels.

### `ILayout<TIdentity, TCoordinate>`

```csharp
public interface ILayout<TIdentity, TCoordinate>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
{
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
- `GetPosition` should be pure and deterministic
- Layouts may require topology at construction for bounds calculation
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
| `IdentityLayout` | For `Point2D`/`Point3D` identities, returns identity as-is |
| `HexLayout` | Maps axial hex coordinates to staggered 2D grid positions |

For non-standard topologies, implement `ILayout<TIdentity, TCoordinate>` with custom logic.

## Rendering

Converts layout positions to visual output. Handles all pixel-level concerns.

### Renderer Responsibilities

- **Grid to pixels**: Cell size, spacing between cells
- **Cell shape**: Square, hexagon, circle (independent of topology)
- **Cell styling**: Colors, characters, borders based on state
- **Output**: Console, image file, animation

### `IRenderer<TIdentity, TState>`

```csharp
public interface IRenderer<TIdentity, TState>
    where TIdentity : notnull, IEquatable<TIdentity>
{
    void Render(
        ILayout<TIdentity, Point2D> layout,
        ITopology<TIdentity> topology,
        IGeneration<TIdentity, TState> generation);
}
```

**Design notes:**
- **2D only**: The interface is typed to `Point2D`. 3D rendering is future work and would require a separate `IRenderer3D` or a generic coordinate parameter.
- **Why topology parameter?** Layout may be constructed from topology, but the renderer needs topology to enumerate nodes. Passing it explicitly avoids coupling layout to a specific topology instance and makes the dependency clear.
- `layout`: Provides grid positions for each node
- `topology`: Provides the list of nodes to render (via `Nodes`)
- `generation`: Provides each node's current state

### `ICellStyle<TState, TVisual>`

Maps cell states to visual representation.

```csharp
public interface ICellStyle<TState, TVisual>
{
    TVisual GetVisual(TState state);
}
```

`TVisual` varies by renderer:

| Renderer | TVisual | Example |
|----------|---------|---------|
| Console | `ConsoleCell` | `('█', Green, Black)` |
| Image | `Color` | `Color.FromRgb(0, 255, 0)` |

### Console Rendering

```csharp
public readonly record struct ConsoleCell(
    char Character,
    ConsoleColor Foreground,
    ConsoleColor Background);

public class BoolConsoleStyle : ICellStyle<bool, ConsoleCell>
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
public class BoolColorStyle : ICellStyle<bool, Color>
{
    public Color Alive { get; init; } = Color.White;
    public Color Dead { get; init; } = Color.Black;

    public Color GetVisual(bool state) => state ? Alive : Dead;
}
```

**Image renderer options:**
- Cell size (pixels)
- Cell shape (square, hexagon, circle)
- Cell spacing/padding
- Border width and color
- Output format (PNG, GIF)
- Animation support (multiple generations → animated GIF)

**Dependencies:**
- Image rendering uses ImageSharp (or similar)
- Kept in separate assembly to avoid core dependency

## Project Structure

*Planned structure. These assemblies will be created when implementation begins. File lists are abbreviated—see interface definitions above for complete API surface.*

```
src/
├── GameOfLife.Core/              # Core library (no rendering)
├── GameOfLife.Rendering/         # Base abstractions
│   ├── ILayout.cs
│   ├── IBounds.cs
│   ├── IRenderer.cs
│   └── ICellStyle.cs
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
var layout = new IdentityLayout<Point2D>(topology);

// Rendering
var style = new BoolConsoleStyle();
var renderer = new ConsoleRenderer<Point2D, bool>(style, cellWidth: 2);

// Game loop
while (true)
{
    renderer.Render(layout, topology, timeline.Current);
    timeline.Step();
    Thread.Sleep(100);
}
```

## Non-Grid Topologies

*TBD* - Rendering arbitrary graph topologies (e.g., named regions, social networks) requires layouts that assign grid positions to non-geometric identities.

**Expected approach:** Callers provide a custom `ILayout<TIdentity, Point2D>` with positions from:
- External layout library (force-directed graph algorithms)
- Explicit dictionary mapping identities to positions
- Domain-specific logic (geographic coordinates)

## Design Decisions

1. **Layout is geometry, rendering is visuals**: Layout produces integer grid coordinates. Rendering handles pixels, shapes, colors.

2. **Integer coordinates for layout**: Grid positions are integers. Pixel conversion (including fractional offsets for hex) happens in the renderer.

3. **Cell shape is a rendering choice**: The same layout can render as squares or hexagons. Topology and layout don't dictate visual shape.

4. **Rendering is pluggable**: Core library has no rendering dependencies. Renderers are separate assemblies.

5. **Style is generic**: `ICellStyle<TState, TVisual>` allows any state type to map to any visual representation.
