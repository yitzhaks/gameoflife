namespace GameOfLife.Library
{
	using System.Diagnostics.CodeAnalysis;

	[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Doesn't make sense for a tile.")]
	public struct Tile
	{
		public bool Alive { get; set; }
	}
}
