namespace GameOfLife.Lib
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using System.Diagnostics;
	using System.Linq;

	using Game.Lib;

	/// <summary>
	/// A hexagonal-shaped board of hexagons.
	/// </summary>
	/// <remarks>
	/// Based on knowledge from https://www.redblobgames.com/grids/hexagons/
	/// </remarks>
	public sealed class HexagonalBoard : IBoard<Point3D, Tile>
	{
		private readonly ImmutableDictionary<Point3D, Tile> m_tiles;

		public HexagonalBoard(int radius)
		{
			if (radius <= 0)
				throw new ArgumentOutOfRangeException(nameof(radius), $"{nameof(Radius)} must be more than 0.");

			Radius = radius;
			var tiles = ImmutableDictionary.CreateBuilder<Point3D, Tile>();

			for (int x = -radius; x <= radius; ++x)
			{
				for (int y = Math.Max(-radius, -x - radius); y <= Math.Min(radius, -x + radius); ++y)
				{
					var z = -x - y;

					tiles.Add(new Point3D(x, y, z), new Tile());
				}
			}

			m_tiles = tiles.ToImmutable();
		}

		/// <summary>
		/// All directions for moving in a circle, starting at the south-west corner.
		/// </summary>
		public static ImmutableArray<Func<Point3D, Point3D>> AllDirections { get; } = ImmutableArray.Create<Func<Point3D, Point3D>>(StepEast, StepNorthEast, StepNorthWest, StepWest, StepSouthWest, StepSouthEast);

		/// <summary>
		/// The coordinates of the center tile of any hexagonal board.
		/// </summary>
		public static Point3D Center => Point3D.Empty; // we use cube coordinates with a center of {X=0, Y=0, Z=0}

		public IEnumerable<(Point3D, Tile)> AllTiles => m_tiles.Select(kvp => (kvp.Key, kvp.Value));

		/// <summary>
		/// Gets the radius of the board.
		/// </summary>
		public int Radius { get; }

		private static int Distance(Point3D a, Point3D b) => (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) + Math.Abs(a.Z - b.Z)) / 2;

		public static Point3D StepEast(Point3D point) => new Point3D(x: point.X + 1, y: point.Y - 1, z: point.Z);
		public static Point3D StepWest(Point3D point) => new Point3D(x: point.X - 1, y: point.Y + 1, z: point.Z);
		public static Point3D StepNorthEast(Point3D point) => new Point3D(x: point.X + 1, y: point.Y, z: point.Z - 1);
		public static Point3D StepNorthWest(Point3D point) => new Point3D(x: point.X, y: point.Y + 1, z: point.Z - 1);
		public static Point3D StepSouthEast(Point3D point) => new Point3D(x: point.X, y: point.Y - 1, z: point.Z + 1);
		public static Point3D StepSouthWest(Point3D point) => new Point3D(x: point.X - 1, y: point.Y, z: point.Z + 1);

		public Tile GetTile(Point3D coordinates)
		{
			ValidateCoordinates(coordinates);

			if (m_tiles.TryGetValue(coordinates, out Tile ret))
				return ret;

			throw new KeyNotFoundException($"There is no tile at the given coordinates {coordinates}. Either invalid coordinates, or out-of-bounds.");
		}

		public IEnumerable<(Point3D, Tile)> GetNeighbors(Point3D coordinates)
		{
			ValidateCoordinates(coordinates);

			foreach (var step in AllDirections)
			{
				var position = step(coordinates);
				if (IsInBounds(position)) // the tile may be out-of-bounds of the board
					yield return (position, GetTile(position));
			}
		}

		[Conditional("DEBUG")]
		private void ValidateCoordinates(Point3D coordinates)
		{
			if (!coordinates.IsValidHexagonalGridCoordinates())
				throw new ArgumentOutOfRangeException(nameof(coordinates), $"The given coordinates {coordinates} are invalid.");

			if (!IsInBounds(coordinates))
				throw new ArgumentOutOfRangeException(nameof(coordinates), $"The given coordinates {coordinates} are out-of-bounds.");
		}

		/// <summary>
		/// Returns a value indicating whether the given coordinates are in the bounds of our board.
		/// </summary>
		/// <param name="coordinates">The coordinates to test.</param>
		/// <returns><c>true</c> if the coordinates are in the bounds of our board; <c>false</c> otherwise.</returns>
		private bool IsInBounds(Point3D coordinates) => Distance(coordinates, Center) <= Radius;
	}

	public static class Point3DExtensions
	{
		public static bool IsValidHexagonalGridCoordinates(this Point3D point) => point.X + point.Y + point.Z == 0;
	}
}
