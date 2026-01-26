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

## Console Rendering Considerations

### Character Aspect Ratio Problem

Console characters are typically taller than they are wide, with a roughly 2:1 height-to-width ratio. When rendering Game of Life patterns where each cell maps to a single character, this causes patterns to appear vertically stretched:

- A glider that should look roughly square appears elongated vertically
- Oscillators like the blinker appear distorted
- Spatial relationships between cells don't match the logical grid topology

This is a fundamental constraint of text-based rendering that affects readability and aesthetic presentation.

### Solution Approaches

There are several approaches to address the aspect ratio problem, each with different trade-offs:

#### 1. Horizontal Doubling

**Approach**: Render each cell as two horizontal characters instead of one.

**Example**:
```
Dead cell: "  " (two spaces)
Alive cell: "██" or "##" (two identical characters)
```

**Pros**:
- Simple to implement
- Produces roughly square cells that match the logical grid topology
- Patterns appear as intended (gliders look like gliders, etc.)

**Cons**:
- Doubles the horizontal screen space required
- May require horizontal scrolling or smaller grid sizes

**Best for**: Standard console rendering where visual accuracy is important.

#### 2. Vertical Subsampling

**Approach**: Render every other row of the logical grid, skipping alternate rows.

**Example**: For a 10x10 logical grid, render only rows 0, 2, 4, 6, 8.

**Pros**:
- No extra horizontal space needed
- Simple implementation

**Cons**:
- Loses information (half the grid is not visible)
- Makes patterns harder to understand
- Asymmetric - horizontal detail is preserved but vertical detail is lost

**Best for**: Quick debugging or when screen space is severely limited.

#### 3. Unicode Block Characters

**Approach**: Use Unicode half-block characters (▀ ▄ █) to encode two vertical cells per character position.

**Example**:
```
Both dead: " " (space)
Top alive, bottom dead: "▀" (upper half block)
Top dead, bottom alive: "▄" (lower half block)
Both alive: "█" (full block)
```

**Pros**:
- Doubles vertical resolution without increasing screen space
- Efficient use of terminal real estate
- Patterns appear approximately square with standard character aspect ratios

**Cons**:
- More complex implementation
- Requires Unicode support
- Two rows share one character position, complicating cursor positioning for interaction
- May have rendering issues in terminals with poor Unicode support

**Best for**: Modern terminals with good Unicode support, when maximizing visible grid area is important.

#### 4. Single Character (Accept Distortion)

**Approach**: Render each cell as a single character and accept the vertical stretching.

**Example**:
```
Dead cell: " " (space)
Alive cell: "█" or "#"
```

**Pros**:
- Simplest implementation
- No special handling needed
- Works in any terminal

**Cons**:
- Patterns appear vertically stretched
- Visual mismatch with logical topology

**Best for**: Prototyping, minimal implementations, or when users understand and accept the distortion.

### Recommendation

For a general-purpose console renderer:

1. **Default**: Use **horizontal doubling** (approach 1) for accuracy and readability.
2. **Optional**: Support **Unicode block characters** (approach 3) as an alternative via constructor/configuration for users with modern terminals who want to maximize grid size.
3. **Document**: Clearly document the aspect ratio handling in the renderer's documentation.

The renderer should be constructed with configuration specifying which approach to use, allowing callers to choose based on their requirements.

## Design Decisions

1. **Layout is geometry, rendering is visuals**: Layout produces logical coordinates. Rendering handles output units, shapes, colors.

2. **Layout coordinates are logical, not output units**: Output conversion (including fractional offsets for hex) happens in the renderer.

3. **Rendering is pluggable**: Core library has no rendering dependencies. Renderers are separate assemblies.
