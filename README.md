# Game of Life

A flexible implementation of Conway's Game of Life that supports arbitrary board topologies.

## Vision

Most Game of Life implementations are limited to square grids. This project aims to break that constraint by treating the board as a **graph** where cells are nodes and neighbor relationships are edges.

This enables:

- **Traditional grids**: Square, hexagonal, triangular
- **3D structures**: Cubic lattices, spherical surfaces
- **Arbitrary topologies**: Doom-like maps with connected sectors, irregular shapes
- **Hybrid boards**: Mix different geometries - connect a hexagonal region to a square grid to a 3D section

The key insight is that the Game of Life algorithm only asks one question: *"Who are my neighbors?"* The geometry is just how we render it.

## Status

This project is in the **design phase**. The architecture has been defined but the implementation has not yet started. See [ARCHITECTURE.md](ARCHITECTURE.md) for the design and [TASKS.md](TASKS.md) for the backlog.

## License

MIT License - see [LICENSE](LICENSE) for details.
