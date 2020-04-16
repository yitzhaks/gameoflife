namespace Game.Library
{
	using System;
	using System.ComponentModel;

	/// <summary>
	/// Represents an ordered pair of integer x-, y-, and z-coordinates that defines a point in a three-dimensional space.
	/// </summary>
	/// <remarks>
	/// The broad strokes of this struct were copied from <see cref="System.Drawing.Point"/>.
	/// </remarks>
	public struct Point3D : IEquatable<Point3D>
	{
		/// <summary>
		/// Represents a Point3D that has <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/> values set to zero.
		/// </summary>
		public static readonly Point3D Empty;

		/// <summary>
		/// Initializes a new instance of the <see cref="Point3D"/> class with the specified coordinates.
		/// </summary>
		/// <param name="x">The x-coordinate of the point.</param>
		/// <param name="y">The y-coordinate of the point.</param>
		/// <param name="z">The z-coordinate of the point.</param>
		public Point3D(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Point3D"/> is empty.
		/// </summary>
		/// <returns>
		/// <c>true</c> if <see cref="X"/>, <see cref="Y"/>, and <see cref="Z"/> are 0; otherwise, <c>false</c>.
		/// </returns>
		[Browsable(false)]
		public bool IsEmpty => X == 0 && Y == 0 && Z == 0;

		/// <summary>
		/// Gets or sets the x-coordinate of this <see cref="Point3D"/>.
		/// </summary>
		/// <returns>
		/// The x-coordinate of this <see cref="Point3D"/>.
		/// </returns>
		public int X { get; set; }

		/// <summary>
		/// Gets or sets the y-coordinate of this <see cref="Point3D"/>.
		/// </summary>
		/// <returns>
		/// The y-coordinate of this <see cref="Point3D"/>.
		/// </returns>
		public int Y { get; set; }

		/// <summary>
		/// Gets or sets the z-coordinate of this <see cref="Point3D"/>.
		/// </summary>
		/// <returns>
		/// The z-coordinate of this <see cref="Point3D"/>.
		/// </returns>
		public int Z { get; set; }

		/// <summary>
		/// Specifies whether this <see cref="Point3D"/> contains the same coordinates as the specified <see cref="object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to test.</param>
		/// <returns><c>true</c> if obj is a <see cref="Point3D"/> and has the same coordinates as this <see cref="Point3D"/>.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Point3D point)
			{
				return Equals(point);
			}

			return false;
		}

		/// <summary>
		/// Specifies whether this <see cref="Point3D"/> contains the same coordinates as the specified <see cref="Point3D"/>.
		/// </summary>
		/// <param name="other">The <see cref="Point3D"/> to test.</param>
		/// <returns><c>true</c> if other has the same coordinates as this <see cref="Point3D"/>.</returns>
		public bool Equals(Point3D other)
		{
			return
				X == other.X &&
				Y == other.Y &&
				Z == other.Z;
		}

		/// <summary>
		/// Returns a hash code for this <see cref="Point3D"/>.
		/// </summary>
		/// <returns>An integer value that specifies a hash value for this <see cref="Point3D"/>.</returns>
		public override int GetHashCode()
		{
			return unchecked(X ^ Y ^ Z);
		}

		/// <summary>
		/// Converts this <see cref="Point3D"/> to a human-readable string.
		/// </summary>
		/// <returns>A string that represents this <see cref="Point3D"/>.</returns>
		public override string ToString()
		{
			return $"{{X={X}, Y={Y}, Z={Z}}}";
		}

		/// <summary>
		/// Compares two <see cref="Point3D"/> objects. The result specifies whether the values of the coordinates of the two <see cref="Point3D"/> objects are equal.
		/// </summary>
		/// <param name="left">A <see cref="Point3D"/> to compare.</param>
		/// <param name="right">A <see cref="Point3D"/> to compare.</param>
		/// <returns><c>true</c> if the coordinates of left and right are equal; otherwise, <c>false</c>.</returns>
		public static bool operator ==(Point3D left, Point3D right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two <see cref="Point3D"/> objects. The result specifies whether the values of the coordinates of the two <see cref="Point3D"/> objects are unequal.
		/// </summary>
		/// <param name="left">A <see cref="Point3D"/> to compare.</param>
		/// <param name="right">A <see cref="Point3D"/> to compare.</param>
		/// <returns><c>true</c> if the coordinates of left and right are differ; otherwise, <c>false</c>.</returns>
		public static bool operator !=(Point3D left, Point3D right)
		{
			return !(left == right);
		}
	}
}
