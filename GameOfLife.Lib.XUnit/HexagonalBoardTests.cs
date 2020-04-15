namespace GameOfLife.Lib.XUnit
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Xunit;

	using Game.Lib;

	public class HexagonalBoardTests
	{
		[Theory]
		[InlineData(3, 37)]
		[InlineData(2, 19)]
		[InlineData(1, 7)]
		public void TileCountIsCorrect(int radius, int size)
		{
			var board = new HexagonalBoard(radius);
			Assert.Equal(size, board.AllTiles.Count());
		}

		[Fact]
		public void NeighborCountIsCorrect()
		{
			const int radius = 3;

			var board = new HexagonalBoard(radius);

			Assert.Equal(6, board.GetNeighbors(HexagonalBoard.Center).Count()); // the center of the board should always have 6 neighbors (since we assume a minimum radius of 1)

			for (int i = 1; i < radius; ++i) // for all rings except the outer ring
			{
				foreach (var tile in TilesOnRing(i)) // all tiles
				{
					Assert.Equal(6, board.GetNeighbors(tile).Count()); // should have 6 neighbors
				}
			}

			foreach (var tile in TilesOnRing(radius)) // for all tiles in the outer ring
			{
				bool IsCorner(Point3D coordinates) => coordinates.X == 0 || coordinates.Y == 0 || coordinates.Z == 0; // this is true only for the outer ring

				int expectedNeighbors = IsCorner(tile) ? 3 : 4;
				Assert.Equal(expectedNeighbors, board.GetNeighbors(tile).Count()); // should have 3/4 neighbors
			}
		}

		private IEnumerable<Point3D> TilesOnRing(int radius)
		{
			var current = new Point3D(-radius, 0, radius); // pick the southwest corner of the ring

			foreach (var step in HexagonalBoard.AllDirections)
			{
				for (int i = 0; i < radius; ++i) // take <radius> steps in each direction
				{
					yield return current; // by yielding before stepping we avoid returning the southwest corner twice

					current = step(current);
				}
			}
		}
	}
}
