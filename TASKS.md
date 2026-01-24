# Task Backlog

## Current Sprint

### Design
- [x] Design and document rendering strategy in [RENDERING.md](RENDERING.md)

### Core Interfaces
- [ ] Create `ITopology<TIdentity>` interface
- [ ] Create `IGeneration<TIdentity, TState>` interface
- [ ] Create `IRules<TState>` interface
- [ ] Create `World<TIdentity, TState>` class
- [ ] Create `Timeline<TIdentity, TState>` class

### First Implementation (Proof of Concept)
- [ ] Implement `Point2D` identity type (immutable, IEquatable)
- [ ] Implement `RectangularTopology` (8-neighbor Moore grid)
- [ ] Implement `ArrayGeneration<TState>` (2D array for bounded grids)
- [ ] Implement `ConwayRules` (classic Game of Life rules)
- [ ] Add integration test: glider pattern advances correctly

## Future Work

### Rendering
- [ ] Implement rendering per [RENDERING.md](RENDERING.md)

### Additional Topologies
- [ ] `HexagonalTopology` (6-neighbor hex grid using cube coordinates)

## Completed

- [x] Define architecture and core abstractions
- [x] Document vision in README.md
- [x] Create ARCHITECTURE.md
- [x] Clean up old code
- [x] Update .editorconfig with modern C# style and analyzers
- [x] Regenerate .gitignore for .NET projects
- [x] Create GitHub Actions CI pipeline (.NET 10, build, test, coverage)
- [x] Create solution and project structure
