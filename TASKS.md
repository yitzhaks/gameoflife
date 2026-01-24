# Task Backlog

## Current Sprint

### Core Interfaces
- [ ] Create `ITopology<TIdentity>` interface
- [ ] Create `IGeneration<TIdentity, TState>` interface
- [ ] Create `IRules<TState>` interface
- [ ] Create `World<TIdentity, TState>` class
- [ ] Create `Timeline<TIdentity, TState>` class

### First Implementation
- [ ] Implement `GraphTopology<TIdentity>` with builder
- [ ] Implement a basic `Generation` (e.g., dictionary-backed)
- [ ] Implement `GameOfLifeRules` (B3/S23)
- [ ] Add unit tests for all core types

## Future Work

### Grid Builders
- [ ] `SquareGridBuilder` - 2D grid (4 or 8 neighbors)
- [ ] `HexGridBuilder` - hexagonal grid (6 neighbors)
- [ ] `CubicGridBuilder` - 3D lattice

### Composite Topologies
- [ ] Design API for joining topologies with portals
- [ ] Implement `CompositeTopology`

### Map-like Boards
- [ ] Design API for Doom-style sector maps
- [ ] Adjacency detection from spatial data

### Alternative Rules
- [ ] HighLife (B36/S23)
- [ ] Day & Night (B3678/S34678)
- [ ] Seeds (B2/S)

### Rendering
- [ ] See [RENDERING.md](RENDERING.md)

## Completed

- [x] Define architecture and core abstractions
- [x] Document vision in README.md
- [x] Create ARCHITECTURE.md
- [x] Clean up old code
