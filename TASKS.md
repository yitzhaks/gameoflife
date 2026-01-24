# Task Backlog

## Current Sprint

### Core Infrastructure
- [ ] Create `ITopology<TCell>` interface
- [ ] Implement `GraphTopology<TCell>` with builder
- [ ] Create `IRules` interface
- [ ] Implement `ClassicRules` (B3/S23)
- [ ] Create `Game<TCell>` class with `Tick()` method
- [ ] Add unit tests for core abstractions

### Grid Builders
- [ ] Implement `SquareGridBuilder`
- [ ] Implement `HexGridBuilder`
- [ ] Add unit tests for grid builders

## Future Work

### Composite Topologies
- [ ] Implement `CompositeTopology` for joining different sections
- [ ] Design portal/connection API
- [ ] Support heterogeneous cell types across sections

### 3D Support
- [ ] Implement `CubicGridBuilder`
- [ ] Consider spherical/toroidal topologies

### Map-like Boards
- [ ] Design API for loading arbitrary maps
- [ ] Support region-based cells (Doom-style sectors)
- [ ] Adjacency detection from spatial data

### Rendering
- [ ] Define `IRenderable<TCell>` interface
- [ ] Console renderer for debugging
- [ ] Consider graphical rendering options

### Rules Variations
- [ ] HighLife (B36/S23)
- [ ] Day & Night (B3678/S34678)
- [ ] Seeds (B2/S)
- [ ] Custom rule parser

## Completed

- [x] Define architecture and core abstractions
- [x] Document vision in README.md
- [x] Create ARCHITECTURE.md
- [x] Clean up old code
