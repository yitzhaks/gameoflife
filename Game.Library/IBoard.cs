namespace Game.Library
{
	using System.Collections.Generic;

	/// <summary>
	/// This interface represents an abstract board of an abstract game.
	/// </summary>
	/// <typeparam name="TCoordinates">The type representing the coordinates of the tiles of the board.</typeparam>
	/// <typeparam name="TTile">The type representing the tiles of the board.</typeparam>
	public interface IBoard<TCoordinates, TTile>
	{
		/// <summary>
		/// Gets all the tiles on the board (and their coordinates).
		/// </summary>
		IEnumerable<(TCoordinates, TTile)> AllTiles { get; }

		/// <summary>
		/// Gets the tile at the given coordinates.
		/// </summary>
		/// <remarks>
		/// Throws an exception if the coordinates are invalid or out-of-bounds.
		/// </remarks>
		/// <param name="coordinates">The coordinates of the tile.</param>
		/// <returns>The tile at the given coordinates.</returns>
		TTile GetTile(TCoordinates coordinates);

		/// <summary>
		/// Gets all the neighbors of the tile at the given coordinates (and their coordinates).
		/// </summary>
		/// <param name="coordinates">The coordinates of the tile.</param>
		/// <returns>An enumerable of all the neighbors.</returns>
		IEnumerable<(TCoordinates, TTile)> GetNeighbors(TCoordinates coordinates);
	}
}
