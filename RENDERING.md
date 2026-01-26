# Rendering

Topology and generation define structure and state, but not geometry or visuals. Rendering adds two layers so the same simulation can be displayed in multiple formats: **Layout** (geometry) and **Renderer** (visuals).

## Core Principle

**Layout decides positions; rendering decides appearance and output.**

- **Layout engine**: Maps topology identities to layout coordinates and builds layout snapshots.
- **Layout**: Immutable snapshot bound to a topology; exposes bounds, positions, and deterministic node ordering.
- **Renderer**: Uses a layout engine to build geometry and converts generation state into concrete output (console/image), applying styling.

Initial implementations target single-frame rendering; animation is documented later as a future improvement.

```
Topology (structure)
    ↓
LayoutEngine (rules)
    ↓
Layout (geometry snapshot) — bounds, ordering, TIdentity → TCoordinate (e.g., Point2D/Point3D)
    ↓
Renderer (visuals) — cell size, shape, colors, output target
```

## Taxonomy

| Concept | Role |
|---------|------|
| LayoutEngine | Builds layout snapshots from topology (mapping rules) |
| Layout | Snapshot bound to topology; maps `TIdentity` → layout position (`Point2D`, `Point3D`, or custom) |
| Bounds | Layout region in coordinates (may be non-rectangular) |
| Renderer | Builds layouts via a layout engine, applies styling, outputs |

## Layout

Layout engines map node identities to layout positions and produce layout snapshots bound to a topology. Positions are logical layout coordinates, not output units.

### `ILayoutEngine<TIdentity, TCoordinate, TBounds>`

```csharp
public interface ILayoutEngine<TIdentity, TCoordinate, TBounds>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
    where TBounds : IBounds<TCoordinate>
{
    ILayout<TIdentity, TCoordinate, TBounds> CreateLayout(ITopology<TIdentity> topology);
}
```

### `ILayout<TIdentity, TCoordinate, TBounds>`

```csharp
public interface ILayout<TIdentity, TCoordinate, TBounds>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
    where TBounds : IBounds<TCoordinate>
{
    IEnumerable<TIdentity> EnumerateNodes(IComparer<TIdentity> order);
    ILayoutPositions<TIdentity, TCoordinate> Positions { get; }
    TBounds Bounds { get; }
}

public interface ILayoutPositions<TIdentity, TCoordinate>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
{
    TCoordinate this[TIdentity node] { get; }
}

public interface IBounds<TCoordinate> where TCoordinate : struct
{
    bool Contains(TCoordinate coordinate);
}

public interface IAxisAlignedBounds<TCoordinate> : IBounds<TCoordinate> where TCoordinate : struct
{
    TCoordinate Min { get; }
    TCoordinate Max { get; }
}
```

**Design notes:**
- `TCoordinate` is a value type representing layout coordinates; `Point2D`/`Point3D` are common examples
- `ILayoutEngine` should be reusable and stateless
- Layouts are bound to the topology used to create them; `EnumerateNodes` should iterate the topology's node set (implementations may forward internally)
- Renderers choose a node ordering via `EnumerateNodes(IComparer<TIdentity>)`
- `Bounds` describes the layout region in coordinate space; all node positions satisfy `Bounds.Contains(position)`
- If the bounds are axis-aligned, `IAxisAlignedBounds` uses inclusive `Min`/`Max` semantics
- `Positions` indexer should be pure and deterministic and throws if the node is unknown to the layout (consistent with `IGeneration` indexer behavior)
- Layouts are immutable snapshots; they should not maintain mutable state and can be cached/reused across generations when topology is unchanged

### Coordinate Types

| Type | Use Case |
|------|----------|
| `Point2D` | Standard 2D grids (rectangular, hex projected to 2D) |
| `Point3D` | 3D visualizations, layered grids |

The difference between square and hex grids is in how identities map to layout positions—the renderer decides the visual shape.

### Standard Implementations

| Implementation | Description |
|----------------|-------------|
| `IdentityLayoutEngine` | For `Point2D`/`Point3D` identities, returns identity as-is |
| `HexLayoutEngine` | Maps cube hex coordinates to staggered 2D layout positions |

For non-standard topologies, implement `ILayoutEngine<TIdentity, TCoordinate, TBounds>` with custom logic.

### Non-Grid Topologies

*TBD* - Rendering arbitrary graph topologies (e.g., named regions, social networks) requires layouts that assign positions to non-geometric identities.

## Rendering

Converts layout positions (built from topology via a layout engine) to visual output. Handles output-specific concerns.

### Renderer Responsibilities

- **Grid to outputs**: Cell size, spacing between cells
- **Node enumeration**: Create a layout from the injected `ILayoutEngine` and use `layout.EnumerateNodes(order)` for deterministic ordering
- **Cell shape**: Square, hexagon, circle (independent of topology)
- **Cell styling**: Colors, characters, borders based on state
- **Output**: Console, image file, animation

### `IRenderer<TIdentity, TCoordinate, TBounds, TState>`

```csharp
public interface IRenderer<TIdentity, TCoordinate, TBounds, TState>
    where TIdentity : notnull, IEquatable<TIdentity>
    where TCoordinate : struct
    where TBounds : IBounds<TCoordinate>
{
    void Render(
        ITopology<TIdentity> topology,
        IGeneration<TIdentity, TState> generation);
}
```

**Design notes:**
- **Coordinate-specific**: Renderers are bound to a coordinate type via `TCoordinate`.
- `TBounds` is part of the renderer contract so the renderer can hold an `ILayoutEngine<TIdentity, TCoordinate, TBounds>`.
- Renderers are constructed with an output target (console, stream, image path, etc.) because `Render()` returns `void`
- Renderers are constructed with an `ILayoutEngine` instance; additional constructor inputs are allowed as needed for the rendering domain
- **Layout is authoritative**: Renderers use the configured `ILayoutEngine` to create a layout for the provided topology, then enumerate `layout.EnumerateNodes(...)`. `generation` is expected to provide a state for each layout node. If a node is missing, treat it as an error (e.g., propagate the exception). Callers using sparse generations should wrap or adapt them to provide default states before rendering.

#### Future improvements

- 3D renderers (e.g., `Point3D`-based rendering)
- Animation and multi-frame rendering are intentionally out of scope for initial implementations.
- Console aspect ratio correction (see below)

## Console Aspect Ratio

Console characters are typically taller than they are wide (roughly 2:1 height-to-width ratio), causing Game of Life patterns to appear vertically stretched when each cell maps to a single character.

### Approaches

**Double-wide cells**: Render each cell as two horizontal characters (`██` instead of `█`). Simple to implement—emit two characters per cell in `TokenEnumerator`. Could be configurable via `--cell-width` option.

**Half-block vertical packing**: Use Unicode half-block characters to represent two vertically adjacent cells in one character position:

| Top Cell | Bottom Cell | Character |
|----------|-------------|-----------|
| Alive    | Alive       | `█` Full block |
| Alive    | Dead        | `▀` Upper half (U+2580) |
| Dead     | Alive       | `▄` Lower half (U+2584) |
| Dead     | Dead        | ` ` Space |

This requires using both foreground and background colors—the half-block's foreground fills one half while the background shows through the other. For example, `▀` with green foreground and dark gray background represents top-alive, bottom-dead.

**Implementation considerations for half-block mode:**
- `AnsiSequence` needs background color codes
- `TokenEnumerator` iterates Y in steps of 2, looks up both cells per position
- Odd-height grids treat the missing bottom row as all-dead
- Rendered height becomes `ceil(gridHeight / 2)`
- `StreamingDiff` must track background color state for differential rendering

**Trade-offs:**
- Double-wide is simpler but doubles horizontal space
- Half-block is more compact but requires background color support and has more complex color logic

Either approach could be exposed via a command-line option (e.g., `--aspect-mode wide|half-block|none`).

## Design Decisions

1. **Layout is geometry, rendering is visuals**: Layout produces logical coordinates. Rendering handles output units, shapes, colors.

2. **Layout coordinates are logical, not output units**: Output conversion (including fractional offsets for hex) happens in the renderer.

3. **Rendering is pluggable**: Core library has no rendering dependencies. Renderers are separate assemblies.
