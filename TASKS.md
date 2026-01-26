# Backlog: Basic Conway's Game of Life

Implementation backlog for Conway's Game of Life with classic B3/S23 rules on a 2D grid.

## Dependency Graph

```
                    ┌─────────────────────────────────────────┐
                    │         PHASE 1: Foundations            │
                    │         (All can run in parallel)       │
                    └─────────────────────────────────────────┘
                                        │
        ┌───────────────┬───────────────┼───────────────┬───────────────┐
        ▼               ▼               ▼               ▼               ▼
   ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
   │ Task 1  │    │ Task 2  │    │ Task 3  │    │ Task 4  │    │ Task 5  │
   │ Point2D │    │ITopology│    │ IGenera-│    │ IRules  │    │ Classic │
   │         │    │         │    │  tion   │    │         │    │  Rules  │
   └────┬────┘    └────┬────┘    └────┬────┘    └────┬────┘    └────┬────┘
        │              │              │              │              │
        │              │              │              │              │
        ▼              ▼              ▼              ▼              ▼
                    ┌─────────────────────────────────────────┐
                    │         PHASE 2: Implementations        │
                    │         (After Phase 1 complete)        │
                    └─────────────────────────────────────────┘
                                        │
        ┌───────────────────────────────┼───────────────────────────────┐
        ▼                               ▼                               ▼
   ┌─────────┐                    ┌─────────┐                    ┌─────────┐
   │ Task 6  │                    │ Task 7  │                    │ Task 8  │
   │ Grid2D  │                    │Dictionar│                    │  World  │
   │Topology │                    │yGenerat.│                    │         │
   └────┬────┘                    └────┬────┘                    └────┬────┘
        │                              │                              │
        └──────────────────────────────┴──────────────────────────────┘
                                        │
                                        ▼
                    ┌─────────────────────────────────────────┐
                    │         PHASE 3: Runner                 │
                    └─────────────────────────────────────────┘
                                        │
                                        ▼
                                   ┌─────────┐
                                   │ Task 9  │
                                   │Timeline │
                                   └─────────┘
```

---

## Phase 1: Foundations (Parallel)

All tasks in this phase can be worked on simultaneously by different agents.

---

### Task 1: Point2D Coordinate Type

**File**: `src/GameOfLife.Core/Point2D.cs`

**Description**: Implement a `Point2D` readonly record struct for 2D coordinates.

**Requirements**:
- Readonly record struct with `int X` and `int Y` properties
- Must implement `IEquatable<Point2D>` (automatic from record struct)
- Value equality semantics (automatic from record struct)
- Consider adding convenience operators or methods if useful

**Tests**: `tests/GameOfLife.Core.Tests/Point2DTests.cs`
- Equality: Same coordinates are equal
- Inequality: Different coordinates are not equal
- Hash code: Equal points have same hash code
- Default value behavior

**Acceptance Criteria**:
- All tests pass
- Near 100% branch coverage

---

### Task 2: ITopology Interface

**File**: `src/GameOfLife.Core/ITopology.cs`

**Description**: Define the `ITopology<TIdentity>` interface as specified in ARCHITECTURE.md.

**Requirements**:
```csharp
public interface ITopology<TIdentity> where TIdentity : notnull, IEquatable<TIdentity>
{
    IEnumerable<TIdentity> Nodes { get; }
    IEnumerable<TIdentity> GetNeighbors(TIdentity node);
}
```

**Tests**: None (interface only)

**Acceptance Criteria**:
- Interface compiles
- Generic constraint is correct

---

### Task 3: IGeneration Interface

**File**: `src/GameOfLife.Core/IGeneration.cs`

**Description**: Define the `IGeneration<TIdentity, TState>` interface as specified in ARCHITECTURE.md.

**Requirements**:
```csharp
public interface IGeneration<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    TState this[TIdentity node] { get; }
}
```

**Tests**: None (interface only)

**Acceptance Criteria**:
- Interface compiles
- Generic constraint is correct

---

### Task 4: IRules Interface

**File**: `src/GameOfLife.Core/IRules.cs`

**Description**: Define the `IRules<TState>` interface as specified in ARCHITECTURE.md.

**Requirements**:
```csharp
public interface IRules<TState>
{
    TState DefaultState { get; }
    TState GetNextState(TState current, IEnumerable<TState> neighborStates);
}
```

**Tests**: None (interface only)

**Acceptance Criteria**:
- Interface compiles

---

### Task 5: ClassicRules (B3/S23)

**File**: `src/GameOfLife.Core/ClassicRules.cs`

**Description**: Implement classic Conway's Game of Life rules (B3/S23) with `TState = bool`.

**Requirements**:
- Implements `IRules<bool>`
- `DefaultState` returns `false` (dead)
- `GetNextState` implements B3/S23:
  - **Birth**: Dead cell with exactly 3 alive neighbors becomes alive
  - **Survival**: Alive cell with 2 or 3 alive neighbors stays alive
  - **Death**: All other cases result in dead

**Tests**: `tests/GameOfLife.Core.Tests/ClassicRulesTests.cs`
- Dead cell with 0-2 neighbors stays dead
- Dead cell with 3 neighbors becomes alive
- Dead cell with 4+ neighbors stays dead
- Alive cell with 0-1 neighbors dies (underpopulation)
- Alive cell with 2 neighbors survives
- Alive cell with 3 neighbors survives
- Alive cell with 4+ neighbors dies (overpopulation)
- DefaultState is false

**Acceptance Criteria**:
- All tests pass
- Near 100% branch coverage

---

## Phase 2: Implementations (Parallel after Phase 1)

These tasks can run in parallel once Phase 1 is complete.

---

### Task 6: Grid2DTopology

**File**: `src/GameOfLife.Core/Grid2DTopology.cs`

**Description**: Implement a finite 2D rectangular grid topology with 8-neighbor adjacency (Moore neighborhood).

**Dependencies**: Task 1 (Point2D), Task 2 (ITopology)

**Requirements**:
- Implements `ITopology<Point2D>`
- Constructor takes `width` and `height`
- `Nodes` returns all points in the grid (0,0) to (width-1, height-1)
- `GetNeighbors` returns up to 8 adjacent cells (fewer at edges/corners)
- Finite boundaries (no wrapping)
- Consider throwing if `GetNeighbors` is called with a node outside the grid

**Tests**: `tests/GameOfLife.Core.Tests/Grid2DTopologyTests.cs`
- Nodes count equals width * height
- Corner cell has 3 neighbors
- Edge cell has 5 neighbors
- Interior cell has 8 neighbors
- Neighbors are symmetric (if A neighbors B, B neighbors A)
- GetNeighbors throws for out-of-bounds node

**Acceptance Criteria**:
- All tests pass
- Near 100% branch coverage

---

### Task 7: DictionaryGeneration

**File**: `src/GameOfLife.Core/DictionaryGeneration.cs`

**Description**: Implement a dictionary-backed generation that stores state sparsely.

**Dependencies**: Task 3 (IGeneration)

**Requirements**:
- Implements `IGeneration<TIdentity, TState>`
- Constructor takes a dictionary of states and a default state for missing nodes
- Indexer returns stored state or default state for unknown nodes
- Consider: Should it throw for unknown nodes? Architecture says "throws if node is unknown" but also mentions sparse storage. **Decision needed**: For basic implementation, store default state explicitly OR track known nodes and throw for truly unknown ones. Recommend: Accept a `ITopology<TIdentity>` or set of valid nodes in constructor, throw if accessing a node not in that set.

**Alternative approach**: Simple implementation that takes `IReadOnlyDictionary<TIdentity, TState>` and `TState defaultState`, returns default for missing keys (simpler, works well for Game of Life where dead cells don't need explicit storage).

**Tests**: `tests/GameOfLife.Core.Tests/DictionaryGenerationTests.cs`
- Returns stored state for known node
- Returns default state for missing node (if using simple approach)
- Throws for unknown node (if using strict approach)
- Works with different TIdentity types

**Acceptance Criteria**:
- All tests pass
- Near 100% branch coverage
- Document the chosen approach in comments

---

### Task 8: World Class

**File**: `src/GameOfLife.Core/World.cs`

**Description**: Implement the World class that combines topology and rules to compute ticks.

**Dependencies**: Task 2 (ITopology), Task 3 (IGeneration), Task 4 (IRules)

**Requirements**:
```csharp
public class World<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    public ITopology<TIdentity> Topology { get; }
    public IRules<TState> Rules { get; }

    public World(ITopology<TIdentity> topology, IRules<TState> rules);
    public IGeneration<TIdentity, TState> Tick(IGeneration<TIdentity, TState> current);
}
```

- `Tick` computes next generation by:
  1. For each node in topology
  2. Get current state from generation
  3. Get neighbor states from generation
  4. Apply rules to get next state
  5. Return new generation with computed states

**Tests**: `tests/GameOfLife.Core.Tests/WorldTests.cs`
- Tick with empty grid stays empty
- Tick applies rules correctly to each cell
- Single cell dies (underpopulation)
- Block pattern (2x2) remains stable (still life)
- Blinker pattern oscillates (period 2)

**Acceptance Criteria**:
- All tests pass
- Near 100% branch coverage

---

## Phase 3: Runner

---

### Task 9: Timeline Class

**File**: `src/GameOfLife.Core/Timeline.cs`

**Description**: Implement the Timeline class that holds a World and current state, enabling stepping through generations.

**Dependencies**: Task 8 (World)

**Requirements**:
```csharp
public class Timeline<TIdentity, TState> where TIdentity : notnull, IEquatable<TIdentity>
{
    public World<TIdentity, TState> World { get; }
    public IGeneration<TIdentity, TState> Current { get; }

    public Timeline(World<TIdentity, TState> world, IGeneration<TIdentity, TState> initial);
    public void Step();           // Advance one generation
    public void Step(int count);  // Advance multiple generations
}
```

- `Step()` advances Current by calling `World.Tick(Current)` and storing the result
- `Step(int count)` calls `Step()` count times
- Consider: Should count <= 0 be a no-op or throw? Recommend: count <= 0 is a no-op

**Tests**: `tests/GameOfLife.Core.Tests/TimelineTests.cs`
- Initial state is accessible via Current
- Step() advances one generation
- Step(n) advances n generations
- Step(0) is a no-op
- Step with negative count is a no-op (or throws, document choice)

**Acceptance Criteria**:
- All tests pass
- Near 100% branch coverage

---

## Summary

| Phase | Tasks | Parallelism |
|-------|-------|-------------|
| 1 | Tasks 1-5 | All 5 can run in parallel |
| 2 | Tasks 6-8 | All 3 can run in parallel |
| 3 | Task 9 | Sequential (depends on Phase 2) |

**Maximum parallelism**: 5 agents in Phase 1, 3 agents in Phase 2.

**Minimum time with unlimited agents**: 3 phases (assuming each phase is atomic).
