using GameOfLife.Core;

namespace GameOfLife.Console;

/// <summary>
/// Analyzes hex patterns to find oscillators and other interesting structures.
/// </summary>
internal static class PatternAnalyzer
{
    public static void AnalyzePatterns(TextWriter output)
    {
        var rules = new HexRulesB2S34();

        output.WriteLine("=== Hex B2S34 Pattern Analysis ===\n");

        // Basic patterns
        output.WriteLine("== Basic Patterns ==\n");
        TestPattern(output, rules, "Horizontal Line (3)", [(0, 0), (1, 0), (-1, 0)]);
        TestPattern(output, rules, "Diagonal Line (3)", [(0, 0), (0, 1), (0, -1)]);
        TestPattern(output, rules, "Bent Line", [(0, 0), (1, 0), (1, -1)]);
        TestPattern(output, rules, "Triangle", [(0, 0), (1, 0), (0, 1)]);
        TestPattern(output, rules, "Hex Ring (6 cells)", [(1, 0), (-1, 0), (0, 1), (0, -1), (1, -1), (-1, 1)]);
        TestPattern(output, rules, "Pair", [(0, 0), (1, 0)]);
        TestPattern(output, rules, "L-Shape", [(0, 0), (1, 0), (0, 1), (0, 2)]);
        TestPattern(output, rules, "Z-Shape", [(0, 0), (1, 0), (0, 1), (-1, 1)]);
        TestPattern(output, rules, "Line of 4", [(0, 0), (1, 0), (-1, 0), (2, 0)]);
        TestPattern(output, rules, "Propeller", [(0, 0), (1, 0), (-1, 0), (0, 1), (1, -1), (-1, 1)]);
        TestPattern(output, rules, "Dense cluster", [(0, 0), (1, 0), (-1, 0), (0, 1), (0, -1), (1, -1), (-1, 1)]);

        // Search for still lifes - patterns where all cells have 3 or 4 neighbors
        output.WriteLine("== Potential Still Lifes ==\n");
        // Double ring - each cell could have 3-4 neighbors
        TestPattern(output, rules, "Double Ring", [
            (2, 0), (-2, 0), (0, 2), (0, -2), (2, -2), (-2, 2),
            (1, 0), (-1, 0), (0, 1), (0, -1), (1, -1), (-1, 1)
        ]);
        // Honeycomb segment
        TestPattern(output, rules, "Honeycomb 7", [
            (0, 0),
            (1, 0), (-1, 0), (0, 1), (0, -1), (1, -1), (-1, 1)
        ]);
        // Triple triangle
        TestPattern(output, rules, "Triple Triangle", [
            (0, 0), (1, 0), (2, 0),
            (0, 1), (1, 1),
            (0, 2)
        ]);
        // Compact cluster
        TestPattern(output, rules, "Compact 4", [(0, 0), (1, 0), (0, 1), (1, -1)]);

        // Search for spaceships by testing asymmetric patterns
        output.WriteLine("== Spaceship Candidates ==\n");
        TestPattern(output, rules, "Arrow", [(0, 0), (1, 0), (0, 1), (-1, 1), (1, -1)]);
        TestPattern(output, rules, "Dart", [(0, 0), (1, 0), (2, 0), (0, 1), (1, -1)]);
        TestPattern(output, rules, "V-Shape", [(0, 0), (1, -1), (-1, 1), (2, -2), (-2, 2)]);
        TestPattern(output, rules, "Fish", [(0, 0), (1, 0), (1, -1), (0, 1), (-1, 1), (-1, 2)]);

        // Test methuselahs (small patterns that evolve for a long time)
        output.WriteLine("== Methuselahs ==\n");
        TestPattern(output, rules, "R-hex", [(0, 0), (1, 0), (0, 1), (-1, 1), (0, 2)]);
        TestPattern(output, rules, "Acorn-hex", [(0, 0), (1, 0), (-1, 0), (1, 1), (0, 2), (2, 0)]);

        // Random search for interesting patterns
        output.WriteLine("== Random Pattern Search ==\n");
        SearchRandomPatterns(output, rules, 5, 100);  // 5 cells, 100 patterns
        SearchRandomPatterns(output, rules, 6, 100);  // 6 cells, 100 patterns
        SearchRandomPatterns(output, rules, 7, 50);   // 7 cells, 50 patterns

        // Detailed view of the Triple Triangle oscillator
        output.WriteLine("== Triple Triangle Oscillator (Period 12) ==\n");
        ShowOscillatorCycle(output, rules, "Triple Triangle", [(0, 0), (1, 0), (2, 0), (0, 1), (1, 1), (0, 2)]);

        output.WriteLine("\n=== Analysis Complete ===");
    }

    private static void ShowOscillatorCycle(TextWriter output, ICellularAutomatonRules rules, string name, HexPoint[] initial)
    {
        output.WriteLine($"--- {name} Full Cycle ---\n");

        var world = new HexagonalWorld(10, rules);
        var states = new List<HashSet<HexPoint>> { new(initial) };

        using var gen0 = new HexGeneration(initial);
        IGeneration<HexPoint, bool> current = gen0;
        IGeneration<HexPoint, bool>? previous = null;

        // Show initial state with ASCII art
        output.WriteLine($"Gen 0 ({initial.Length} cells):");
        DrawHexPattern(output, initial);
        output.WriteLine();

        for (int i = 1; i <= 20; i++)
        {
            IGeneration<HexPoint, bool> next = world.Tick(current);
            var alive = ((HexGeneration)next).AliveCells.ToHashSet();

            output.WriteLine($"Gen {i} ({alive.Count} cells):");
            DrawHexPattern(output, alive);
            output.WriteLine();

            // Check for cycle completion
            for (int j = 0; j < states.Count; j++)
            {
                if (states[j].SetEquals(alive))
                {
                    int period = i - j;
                    output.WriteLine($"*** Cycle complete! Period = {period} (returned to gen {j}) ***\n");
                    previous?.Dispose();
                    next.Dispose();
                    return;
                }
            }

            states.Add(alive);
            previous?.Dispose();
            previous = current;
            current = next;
        }

        previous?.Dispose();
        if (!ReferenceEquals(current, gen0))
        {
            current.Dispose();
        }
    }

    private static void DrawHexPattern(TextWriter output, IEnumerable<HexPoint> cells)
    {
        var cellSet = cells.ToHashSet();
        if (cellSet.Count == 0)
        {
            output.WriteLine("  (empty)");
            return;
        }

        int minQ = cellSet.Min(c => c.Q);
        int maxQ = cellSet.Max(c => c.Q);
        int minR = cellSet.Min(c => c.R);
        int maxR = cellSet.Max(c => c.R);

        for (int r = minR; r <= maxR; r++)
        {
            // Indent for hex staggering
            int indent = r - minR;
            output.Write(new string(' ', indent));

            for (int q = minQ; q <= maxQ; q++)
            {
                if (cellSet.Contains((q, r)))
                {
                    output.Write("@ ");
                }
                else
                {
                    output.Write(". ");
                }
            }

            output.WriteLine();
        }
    }

    private static void SearchRandomPatterns(TextWriter output, ICellularAutomatonRules rules, int cellCount, int trials)
    {
        output.WriteLine($"Searching {trials} random patterns with {cellCount} cells...");

#pragma warning disable CA5394 // Do not use insecure randomness - this is for game simulation
        var random = new Random(42);  // Fixed seed for reproducibility
        var foundPatterns = new Dictionary<string, (int period, int cells, HexPoint[] initial)>();

        for (int trial = 0; trial < trials; trial++)
        {
            // Generate random pattern within a small area
            var cells = new HashSet<HexPoint>();
            while (cells.Count < cellCount)
            {
                int q = random.Next(-3, 4);
                int r = random.Next(-3, 4);
                _ = cells.Add((q, r));
            }
#pragma warning restore CA5394

            HexPoint[] initial = [.. cells];
            (int period, int cells)? result = AnalyzePatternSilent(rules, initial);

            if (result.HasValue)
            {
                string key = $"{result.Value.period}-{result.Value.cells}";
                if (!foundPatterns.ContainsKey(key))
                {
                    foundPatterns[key] = (result.Value.period, result.Value.cells, initial);
                }
            }
        }

        // Report findings
        foreach (KeyValuePair<string, (int period, int cells, HexPoint[] initial)> kvp in foundPatterns.OrderBy(x => x.Value.period).ThenBy(x => x.Value.cells))
        {
            if (kvp.Value.period == 1)
            {
                output.WriteLine($"  STILL LIFE ({kvp.Value.cells} cells): {FormatCells(kvp.Value.initial)}");
            }
            else
            {
                output.WriteLine($"  Period {kvp.Value.period} ({kvp.Value.cells} cells): from {FormatCells(kvp.Value.initial)}");
            }
        }

        output.WriteLine();
    }

    private static (int period, int cells)? AnalyzePatternSilent(ICellularAutomatonRules rules, HexPoint[] initial)
    {
        var world = new HexagonalWorld(10, rules);
        var states = new List<HashSet<HexPoint>> { new(initial) };

        using var gen0 = new HexGeneration(initial);
        IGeneration<HexPoint, bool> current = gen0;
        IGeneration<HexPoint, bool>? previous = null;

        for (int i = 1; i <= 50; i++)
        {
            IGeneration<HexPoint, bool> next = world.Tick(current);
            var alive = ((HexGeneration)next).AliveCells.ToHashSet();

            for (int j = 0; j < states.Count; j++)
            {
                if (states[j].SetEquals(alive))
                {
                    int period = i - j;
                    previous?.Dispose();
                    next.Dispose();
                    return (period, alive.Count);
                }
            }

            if (alive.Count is 0 or > 100)
            {
                previous?.Dispose();
                next.Dispose();
                return null;  // Died or exploded
            }

            states.Add(alive);
            previous?.Dispose();
            previous = current;
            current = next;
        }

        previous?.Dispose();
        if (!ReferenceEquals(current, gen0))
        {
            current.Dispose();
        }

        return null;  // Still evolving
    }

    private static void TestPattern(TextWriter output, ICellularAutomatonRules rules, string name, HexPoint[] initial)
    {
        output.WriteLine($"--- {name} ---");
        output.WriteLine($"Initial ({initial.Length} cells): {FormatCells(initial)}");

        var world = new HexagonalWorld(10, rules);
        var states = new List<HashSet<HexPoint>>
        {
            new(initial)
        };

        using var gen0 = new HexGeneration(initial);
        IGeneration<HexPoint, bool> current = gen0;
        IGeneration<HexPoint, bool>? previous = null;

        for (int i = 1; i <= 50; i++)
        {
            IGeneration<HexPoint, bool> next = world.Tick(current);
            var alive = ((HexGeneration)next).AliveCells.ToHashSet();

            // Check for oscillation
            for (int j = 0; j < states.Count; j++)
            {
                if (states[j].SetEquals(alive))
                {
                    int period = i - j;
                    if (period == 1 && alive.Count > 0)
                    {
                        output.WriteLine($"  -> STILL LIFE at gen {j} ({alive.Count} cells): {FormatCells(alive)}");
                    }
                    else if (period > 1)
                    {
                        output.WriteLine($"  -> OSCILLATOR! Period {period}, detected at gen {i} ({alive.Count} cells)");
                        output.WriteLine($"     States: gen {j} and gen {i}");
                    }

                    previous?.Dispose();
                    next.Dispose();
                    output.WriteLine();
                    return;
                }
            }

            if (alive.Count == 0)
            {
                output.WriteLine($"  -> DIED at gen {i}");
                previous?.Dispose();
                next.Dispose();
                output.WriteLine();
                return;
            }

            if (alive.Count > 100)
            {
                output.WriteLine($"  -> EXPLODING (>{alive.Count} cells at gen {i})");
                previous?.Dispose();
                next.Dispose();
                output.WriteLine();
                return;
            }

            states.Add(alive);
            previous?.Dispose();
            previous = current;
            current = next;
        }

        IReadOnlyCollection<HexPoint> finalCells = ((HexGeneration)current).AliveCells;
        output.WriteLine($"  -> Still evolving after 50 gens ({finalCells.Count} cells)");
        previous?.Dispose();
        if (!ReferenceEquals(current, gen0))
        {
            current.Dispose();
        }

        output.WriteLine();
    }

    private static string FormatCells(IEnumerable<HexPoint> cells) =>
        string.Join(" ", cells.OrderBy(p => p.R).ThenBy(p => p.Q).Select(p => $"({p.Q},{p.R})"));
}
