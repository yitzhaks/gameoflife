# Rendering

Topology defines structure, not visual position. Rendering requires additional metadata to display nodes on screen.

## Core Principle

**Rendering is separate from simulation.**

The Game of Life engine (World, Timeline) knows nothing about pixels, characters, or colors. Rendering is a separate layer that:
1. Maps node identities to screen positions (Layout)
2. Maps cell states to visual appearance (StateStyle)
3. Produces output for a specific target (Renderer)

## Taxonomy

| Concept | Role |
|---------|------|
| Layout | Maps `TIdentity` → screen position |
| StateStyle | Maps `TState` → visual appearance |
| Renderer | Combines layout + style → output |

## Core Abstractions

### ILayout<TIdentity>

Maps node identities to screen positions. Positions are in a logical coordinate space (not pixels).

```csharp
public interface ILayout<TIdentity> where TIdentity : notnull, IEquatable<TIdentity>
{
    LayoutPosition GetPosition(TIdentity node);
    LayoutBounds Bounds { get; }
}

public readonly record struct LayoutPosition(double X, double Y);

public readonly record struct LayoutBounds(double Width, double Height);
```

**Design notes:**
- Uses `double` for positions to support fractional/scaled layouts
- `Bounds` gives the overall size for viewport/canvas sizing
- Implementations compute position from identity (e.g., `Point2D(3,4)` → `LayoutPosition(3, 4)`)
- Layouts may require topology at construction time for bounds calculation, even though position mapping is identity-based
- `GetPosition` should be pure and deterministic - same identity always returns same position. Layouts should not maintain mutable state.

#### Standard Implementations

| Implementation | Description |
|----------------|-------------|
| `GridLayout` | Maps `Point2D` directly to positions (1:1) |
| `ScaledGridLayout` | Maps `Point2D` with configurable cell size |
| `HexLayout` | Maps hex coordinates to staggered positions |

For non-standard topologies, implement `ILayout<TIdentity>` with custom logic.

### IStateStyle<TState, TVisual>

Maps cell states to visual representation. Abstract over output format.

```csharp
public interface IStateStyle<TState, TVisual>
{
    TVisual GetVisual(TState state);
}
```

`TVisual` varies by renderer:

| Renderer | TVisual | Example |
|----------|---------|---------|
| Console | `ConsoleCell` | `('█', ConsoleColor.Green)` |
| Image | `Color` | `Color.FromRgb(0, 255, 0)` |

#### Console Visuals

```csharp
public readonly record struct ConsoleCell(char Character, ConsoleColor Foreground, ConsoleColor Background);

public class BoolConsoleStyle : IStateStyle<bool, ConsoleCell>
{
    public ConsoleCell Alive { get; init; } = new('█', ConsoleColor.Green, ConsoleColor.Black);
    public ConsoleCell Dead { get; init; } = new(' ', ConsoleColor.Black, ConsoleColor.Black);

    public ConsoleCell GetVisual(bool state) => state ? Alive : Dead;
}
```

#### Image Visuals

```csharp
public class BoolColorStyle : IStateStyle<bool, Color>
{
    public Color Alive { get; init; } = Color.White;
    public Color Dead { get; init; } = Color.Black;

    public Color GetVisual(bool state) => state ? Alive : Dead;
}
```

### IRenderer<TIdentity, TState>

Renders a generation to a specific output target.

```csharp
public interface IRenderer<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    void Render(ITopology<TIdentity> topology, IGeneration<TIdentity, TState> generation);
}
```

**Why both topology and generation?** Topology provides the list of nodes to render (via `Nodes`). Generation provides each node's current state. They are independent: topology is structural and immutable, while generation changes each tick. Passing both keeps the interface explicit and avoids coupling generation to topology.

Renderers are configured with layout and style at construction time:

```csharp
public class ConsoleRenderer<TIdentity, TState> : IRenderer<TIdentity, TState>
    where TIdentity : notnull, IEquatable<TIdentity>
{
    public ConsoleRenderer(ILayout<TIdentity> layout, IStateStyle<TState, ConsoleCell> style) { ... }

    public void Render(ITopology<TIdentity> topology, IGeneration<TIdentity, TState> generation) { ... }
}
```

## Output Targets

### Console

Renders to terminal using characters and colors.

**Features:**
- Cursor positioning for in-place updates
- Configurable characters and colors via `IStateStyle<TState, ConsoleCell>`
- Respects terminal dimensions

**Limitations:**
- Character aspect ratio (~2:1) distorts square grids
- Limited color palette (16 colors standard, 256 with ANSI)

### Image

Renders to image files (PNG, GIF).

**Features:**
- Configurable cell size in pixels
- True color support
- Animation support (GIF) for multiple generations

**Dependencies:**
- Image rendering will use a lightweight library (e.g., ImageSharp)
- Kept in a separate assembly to avoid core dependency

## Project Structure

*Planned structure. Rendering assemblies will be created when implementation begins.*

```
src/
├── GameOfLife.Core/              # Core library (no rendering)
├── GameOfLife.Rendering/         # Base rendering abstractions
│   ├── ILayout.cs
│   ├── IStateStyle.cs
│   ├── IRenderer.cs
│   ├── LayoutPosition.cs
│   ├── LayoutBounds.cs
│   └── Layouts/
│       └── GridLayout.cs
├── GameOfLife.Rendering.Console/ # Console renderer
│   ├── ConsoleCell.cs
│   ├── ConsoleRenderer.cs
│   └── Styles/
│       └── BoolConsoleStyle.cs
└── GameOfLife.Rendering.Image/   # Image renderer (ImageSharp dependency)
    ├── ImageRenderer.cs
    └── Styles/
        └── BoolColorStyle.cs
```

## Usage Example

```csharp
// Setup
var topology = new RectangularTopology(width: 40, height: 20);
var world = new World<Point2D, bool>(topology, new ConwayRules());
var timeline = new Timeline<Point2D, bool>(world, initialGeneration);

// Rendering
var layout = new GridLayout(topology);
var style = new BoolConsoleStyle();
var renderer = new ConsoleRenderer<Point2D, bool>(layout, style);

// Game loop
while (true)
{
    renderer.Render(topology, timeline.Current);
    timeline.Step();
    Thread.Sleep(100);
}
```

## Non-Grid Topologies

*TBD* - Rendering arbitrary graph topologies (e.g., named regions, social networks) requires different approaches:
- Force-directed layout algorithms
- Manual position specification
- Domain-specific layout strategies

These will be addressed when such topologies are implemented.

**Expected approach:** Callers will provide a custom `ILayout<TIdentity>` implementation with positions either:
- Computed via an external layout library (e.g., force-directed graph layout)
- Specified explicitly via a dictionary mapping identities to positions
- Derived from domain-specific logic (e.g., geographic coordinates)

## Design Decisions

1. **Rendering is pluggable**: Core library has no rendering dependencies. Renderers are separate assemblies.

2. **Layout is computed**: Default layouts derive position from identity. Custom layouts can override for special cases.

3. **Style is generic**: `IStateStyle<TState, TVisual>` allows any state type to map to any visual representation.

4. **Renderers own their dependencies**: `ConsoleRenderer` knows about console APIs. `ImageRenderer` knows about image libraries. The interface stays clean.

5. **Double precision for positions**: Allows fractional positions for hex grids, scaled layouts, and smooth animations without integer rounding issues.
