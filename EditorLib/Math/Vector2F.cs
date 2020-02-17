using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;


namespace AuxLib
{
	

	/// <summary>
	/// Describes a 2D-vector.
	/// </summary>
	[DataContract]
	[DebuggerDisplay("{DebugDisplayString,nq}")]
	public struct Vector2f : IEquatable<Vector2f>
	{
		#region Private Fields

		private static readonly Vector2f zeroVector = new Vector2f(0f, 0f);
		private static readonly Vector2f unitVector = new Vector2f(1f, 1f);
		private static readonly Vector2f unitXVector = new Vector2f(1f, 0f);
		private static readonly Vector2f unitYVector = new Vector2f(0f, 1f);

		#endregion

		#region Public Fields

		/// <summary>
		/// The x coordinate of this <see cref="Vector2f"/>.
		/// </summary>
		[DataMember]
		public float X;

		/// <summary>
		/// The y coordinate of this <see cref="Vector2f"/>.
		/// </summary>
		[DataMember]
		public float Y;

		#endregion

		#region Properties

		/// <summary>
		/// Returns a <see cref="Vector2f"/> with components 0, 0.
		/// </summary>
		public static Vector2f Zero
		{
			get { return zeroVector; }
		}

		/// <summary>
		/// Returns a <see cref="Vector2f"/> with components 1, 1.
		/// </summary>
		public static Vector2f One
		{
			get { return unitVector; }
		}

		/// <summary>
		/// Returns a <see cref="Vector2f"/> with components 1, 0.
		/// </summary>
		public static Vector2f UnitX
		{
			get { return unitXVector; }
		}

		/// <summary>
		/// Returns a <see cref="Vector2f"/> with components 0, 1.
		/// </summary>
		public static Vector2f UnitY
		{
			get { return unitYVector; }
		}

		#endregion

		#region Internal Properties

		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					this.X.ToString(), "  ",
					this.Y.ToString()
				);
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a 2d vector with X and Y from two values.
		/// </summary>
		/// <param name="x">The x coordinate in 2d-space.</param>
		/// <param name="y">The y coordinate in 2d-space.</param>
		public Vector2f(float x, float y)
		{
			this.X = x;
			this.Y = y;
		}

		/// <summary>
		/// Constructs a 2d vector with X and Y set to the same value.
		/// </summary>
		/// <param name="value">The x and y coordinates in 2d-space.</param>
		public Vector2f(float value)
		{
			this.X = value;
			this.Y = value;
		}

		#endregion

		#region Operators

		/// <summary>
		/// Inverts values in the specified <see cref="Vector2f"/>.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2f"/> on the right of the sub sign.</param>
		/// <returns>Result of the inversion.</returns>
		public static Vector2f operator -(Vector2f value)
		{
			value.X = -value.X;
			value.Y = -value.Y;
			return value;
		}

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/> on the left of the add sign.</param>
		/// <param name="value2">Source <see cref="Vector2f"/> on the right of the add sign.</param>
		/// <returns>Sum of the vectors.</returns>
		public static Vector2f operator +(Vector2f value1, Vector2f value2)
		{
			value1.X += value2.X;
			value1.Y += value2.Y;
			return value1;
		}

		/// <summary>
		/// Subtracts a <see cref="Vector2f"/> from a <see cref="Vector2f"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/> on the left of the sub sign.</param>
		/// <param name="value2">Source <see cref="Vector2f"/> on the right of the sub sign.</param>
		/// <returns>Result of the vector subtraction.</returns>
		public static Vector2f operator -(Vector2f value1, Vector2f value2)
		{
			value1.X -= value2.X;
			value1.Y -= value2.Y;
			return value1;
		}

		/// <summary>
		/// Multiplies the components of two vectors by each other.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/> on the left of the mul sign.</param>
		/// <param name="value2">Source <see cref="Vector2f"/> on the right of the mul sign.</param>
		/// <returns>Result of the vector multiplication.</returns>
		public static Vector2f operator *(Vector2f value1, Vector2f value2)
		{
			value1.X *= value2.X;
			value1.Y *= value2.Y;
			return value1;
		}

		/// <summary>
		/// Multiplies the components of vector by a scalar.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2f"/> on the left of the mul sign.</param>
		/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
		/// <returns>Result of the vector multiplication with a scalar.</returns>
		public static Vector2f operator *(Vector2f value, float scaleFactor)
		{
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			return value;
		}

		/// <summary>
		/// Multiplies the components of vector by a scalar.
		/// </summary>
		/// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
		/// <param name="value">Source <see cref="Vector2f"/> on the right of the mul sign.</param>
		/// <returns>Result of the vector multiplication with a scalar.</returns>
		public static Vector2f operator *(float scaleFactor, Vector2f value)
		{
			value.X *= scaleFactor;
			value.Y *= scaleFactor;
			return value;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2f"/> by the components of another <see cref="Vector2f"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/> on the left of the div sign.</param>
		/// <param name="value2">Divisor <see cref="Vector2f"/> on the right of the div sign.</param>
		/// <returns>The result of dividing the vectors.</returns>
		public static Vector2f operator /(Vector2f value1, Vector2f value2)
		{
			value1.X /= value2.X;
			value1.Y /= value2.Y;
			return value1;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2f"/> by a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/> on the left of the div sign.</param>
		/// <param name="divider">Divisor scalar on the right of the div sign.</param>
		/// <returns>The result of dividing a vector by a scalar.</returns>
		public static Vector2f operator /(Vector2f value1, float divider)
		{
			float factor = 1 / divider;
			value1.X *= factor;
			value1.Y *= factor;
			return value1;
		}

		/// <summary>
		/// Compares whether two <see cref="Vector2f"/> instances are equal.
		/// </summary>
		/// <param name="value1"><see cref="Vector2f"/> instance on the left of the equal sign.</param>
		/// <param name="value2"><see cref="Vector2f"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==(Vector2f value1, Vector2f value2)
		{
			return value1.X == value2.X && value1.Y == value2.Y;
		}

		/// <summary>
		/// Compares whether two <see cref="Vector2f"/> instances are not equal.
		/// </summary>
		/// <param name="value1"><see cref="Vector2f"/> instance on the left of the not equal sign.</param>
		/// <param name="value2"><see cref="Vector2f"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
		public static bool operator !=(Vector2f value1, Vector2f value2)
		{
			return value1.X != value2.X || value1.Y != value2.Y;
		}

		#endregion

		#region Public Methods

        public Vector2 ToVector2()
        {
            return new Vector2((int)X, (int)Y);
        }

		/// <summary>
		/// Performs vector addition on <paramref name="value1"/> and <paramref name="value2"/>.
		/// </summary>
		/// <param name="value1">The first vector to add.</param>
		/// <param name="value2">The second vector to add.</param>
		/// <returns>The result of the vector addition.</returns>
		public static Vector2f Add(Vector2f value1, Vector2f value2)
		{
			value1.X += value2.X;
			value1.Y += value2.Y;
			return value1;
		}

		/// <summary>
		/// Performs vector addition on <paramref name="value1"/> and
		/// <paramref name="value2"/>, storing the result of the
		/// addition in <paramref name="result"/>.
		/// </summary>
		/// <param name="value1">The first vector to add.</param>
		/// <param name="value2">The second vector to add.</param>
		/// <param name="result">The result of the vector addition.</param>
		public static void Add(ref Vector2f value1, ref Vector2f value2, out Vector2f result)
		{
			result.X = value1.X + value2.X;
			result.Y = value1.Y + value2.Y;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 2d-triangle.
		/// </summary>
		/// <param name="value1">The first vector of 2d-triangle.</param>
		/// <param name="value2">The second vector of 2d-triangle.</param>
		/// <param name="value3">The third vector of 2d-triangle.</param>
		/// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 2d-triangle.</param>
		/// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 2d-triangle.</param>
		/// <returns>The cartesian translation of barycentric coordinates.</returns>
		public static Vector2f Barycentric(Vector2f value1, Vector2f value2, Vector2f value3, float amount1, float amount2)
		{
			return new Vector2f(
				Maths.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
				Maths.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 2d-triangle.
		/// </summary>
		/// <param name="value1">The first vector of 2d-triangle.</param>
		/// <param name="value2">The second vector of 2d-triangle.</param>
		/// <param name="value3">The third vector of 2d-triangle.</param>
		/// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 2d-triangle.</param>
		/// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 2d-triangle.</param>
		/// <param name="result">The cartesian translation of barycentric coordinates as an output parameter.</param>
		public static void Barycentric(ref Vector2f value1, ref Vector2f value2, ref Vector2f value3, float amount1, float amount2, out Vector2f result)
		{
			result.X = Maths.Barycentric(value1.X, value2.X, value3.X, amount1, amount2);
			result.Y = Maths.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains CatmullRom interpolation of the specified vectors.
		/// </summary>
		/// <param name="value1">The first vector in interpolation.</param>
		/// <param name="value2">The second vector in interpolation.</param>
		/// <param name="value3">The third vector in interpolation.</param>
		/// <param name="value4">The fourth vector in interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>The result of CatmullRom interpolation.</returns>
		public static Vector2f CatmullRom(Vector2f value1, Vector2f value2, Vector2f value3, Vector2f value4, float amount)
		{
			return new Vector2f(
				Maths.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
				Maths.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains CatmullRom interpolation of the specified vectors.
		/// </summary>
		/// <param name="value1">The first vector in interpolation.</param>
		/// <param name="value2">The second vector in interpolation.</param>
		/// <param name="value3">The third vector in interpolation.</param>
		/// <param name="value4">The fourth vector in interpolation.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <param name="result">The result of CatmullRom interpolation as an output parameter.</param>
		public static void CatmullRom(ref Vector2f value1, ref Vector2f value2, ref Vector2f value3, ref Vector2f value4, float amount, out Vector2f result)
		{
			result.X = Maths.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount);
			result.Y = Maths.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount);
		}

		/// <summary>
		/// Clamps the specified value within a range.
		/// </summary>
		/// <param name="value1">The value to clamp.</param>
		/// <param name="min">The min value.</param>
		/// <param name="max">The max value.</param>
		/// <returns>The clamped value.</returns>
		public static Vector2f Clamp(Vector2f value1, Vector2f min, Vector2f max)
		{
			return new Vector2f(
				Maths.Clamp(value1.X, min.X, max.X),
				Maths.Clamp(value1.Y, min.Y, max.Y));
		}

		/// <summary>
		/// Clamps the specified value within a range.
		/// </summary>
		/// <param name="value1">The value to clamp.</param>
		/// <param name="min">The min value.</param>
		/// <param name="max">The max value.</param>
		/// <param name="result">The clamped value as an output parameter.</param>
		public static void Clamp(ref Vector2f value1, ref Vector2f min, ref Vector2f max, out Vector2f result)
		{
			result.X = Maths.Clamp(value1.X, min.X, max.X);
			result.Y = Maths.Clamp(value1.Y, min.Y, max.Y);
		}

		/// <summary>
		/// Returns the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The distance between two vectors.</returns>
		public static float Distance(Vector2f value1, Vector2f value2)
		{
			float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
			return (float)Math.Sqrt((v1 * v1) + (v2 * v2));
		}

		/// <summary>
		/// Returns the distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The distance between two vectors as an output parameter.</param>
		public static void Distance(ref Vector2f value1, ref Vector2f value2, out float result)
		{
			float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
			result = (float)Math.Sqrt((v1 * v1) + (v2 * v2));
		}

		/// <summary>
		/// Returns the squared distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The squared distance between two vectors.</returns>
		public static float DistanceSquared(Vector2f value1, Vector2f value2)
		{
			float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
			return (v1 * v1) + (v2 * v2);
		}

		/// <summary>
		/// Returns the squared distance between two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The squared distance between two vectors as an output parameter.</param>
		public static void DistanceSquared(ref Vector2f value1, ref Vector2f value2, out float result)
		{
			float v1 = value1.X - value2.X, v2 = value1.Y - value2.Y;
			result = (v1 * v1) + (v2 * v2);
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2f"/> by the components of another <see cref="Vector2f"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="value2">Divisor <see cref="Vector2f"/>.</param>
		/// <returns>The result of dividing the vectors.</returns>
		public static Vector2f Divide(Vector2f value1, Vector2f value2)
		{
			value1.X /= value2.X;
			value1.Y /= value2.Y;
			return value1;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2f"/> by the components of another <see cref="Vector2f"/>.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="value2">Divisor <see cref="Vector2f"/>.</param>
		/// <param name="result">The result of dividing the vectors as an output parameter.</param>
		public static void Divide(ref Vector2f value1, ref Vector2f value2, out Vector2f result)
		{
			result.X = value1.X / value2.X;
			result.Y = value1.Y / value2.Y;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2f"/> by a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <returns>The result of dividing a vector by a scalar.</returns>
		public static Vector2f Divide(Vector2f value1, float divider)
		{
			float factor = 1 / divider;
			value1.X *= factor;
			value1.Y *= factor;
			return value1;
		}

		/// <summary>
		/// Divides the components of a <see cref="Vector2f"/> by a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="divider">Divisor scalar.</param>
		/// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
		public static void Divide(ref Vector2f value1, float divider, out Vector2f result)
		{
			float factor = 1 / divider;
			result.X = value1.X * factor;
			result.Y = value1.Y * factor;
		}

		/// <summary>
		/// Returns a dot product of two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The dot product of two vectors.</returns>
		public static float Dot(Vector2f value1, Vector2f value2)
		{
			return (value1.X * value2.X) + (value1.Y * value2.Y);
		}

		/// <summary>
		/// Returns a dot product of two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The dot product of two vectors as an output parameter.</param>
		public static void Dot(ref Vector2f value1, ref Vector2f value2, out float result)
		{
			result = (value1.X * value2.X) + (value1.Y * value2.Y);
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Vector2f)
			{
				return Equals((Vector2f)obj);
			}

			return false;
		}

		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Vector2f"/>.
		/// </summary>
		/// <param name="other">The <see cref="Vector2f"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals(Vector2f other)
		{
			return (X == other.X) && (Y == other.Y);
		}

		/// <summary>
		/// Gets the hash code of this <see cref="Vector2f"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="Vector2f"/>.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode();
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains hermite spline interpolation.
		/// </summary>
		/// <param name="value1">The first position vector.</param>
		/// <param name="tangent1">The first tangent vector.</param>
		/// <param name="value2">The second position vector.</param>
		/// <param name="tangent2">The second tangent vector.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <returns>The hermite spline interpolation vector.</returns>
		public static Vector2f Hermite(Vector2f value1, Vector2f tangent1, Vector2f value2, Vector2f tangent2, float amount)
		{
			return new Vector2f(Maths.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount), Maths.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount));
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains hermite spline interpolation.
		/// </summary>
		/// <param name="value1">The first position vector.</param>
		/// <param name="tangent1">The first tangent vector.</param>
		/// <param name="value2">The second position vector.</param>
		/// <param name="tangent2">The second tangent vector.</param>
		/// <param name="amount">Weighting factor.</param>
		/// <param name="result">The hermite spline interpolation vector as an output parameter.</param>
		public static void Hermite(ref Vector2f value1, ref Vector2f tangent1, ref Vector2f value2, ref Vector2f tangent2, float amount, out Vector2f result)
		{
			result.X = Maths.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
			result.Y = Maths.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
		}

		/// <summary>
		/// Returns the length of this <see cref="Vector2f"/>.
		/// </summary>
		/// <returns>The length of this <see cref="Vector2f"/>.</returns>
		public float Length()
		{
			return (float)Math.Sqrt((X * X) + (Y * Y));
		}

		/// <summary>
		/// Returns the squared length of this <see cref="Vector2f"/>.
		/// </summary>
		/// <returns>The squared length of this <see cref="Vector2f"/>.</returns>
		public float LengthSquared()
		{
			return (X * X) + (Y * Y);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains linear interpolation of the specified vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <returns>The result of linear interpolation of the specified vectors.</returns>
		public static Vector2f Lerp(Vector2f value1, Vector2f value2, float amount)
		{
			return new Vector2f(
				Maths.Lerp(value1.X, value2.X, amount),
				Maths.Lerp(value1.Y, value2.Y, amount));
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains linear interpolation of the specified vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
		public static void Lerp(ref Vector2f value1, ref Vector2f value2, float amount, out Vector2f result)
		{
			result.X = Maths.Lerp(value1.X, value2.X, amount);
			result.Y = Maths.Lerp(value1.Y, value2.Y, amount);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains linear interpolation of the specified vectors.
		/// Uses <see cref="Maths.LerpPrecise"/> on MathHelper for the interpolation.
		/// Less efficient but more precise compared to <see cref="Vector2f.Lerp(Vector2f, Vector2f, float)"/>.
		/// See remarks section of <see cref="Maths.LerpPrecise"/> on MathHelper for more info.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <returns>The result of linear interpolation of the specified vectors.</returns>
		public static Vector2f LerpPrecise(Vector2f value1, Vector2f value2, float amount)
		{
			return new Vector2f(
				Maths.LerpPrecise(value1.X, value2.X, amount),
				Maths.LerpPrecise(value1.Y, value2.Y, amount));
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains linear interpolation of the specified vectors.
		/// Uses <see cref="Maths.LerpPrecise"/> on MathHelper for the interpolation.
		/// Less efficient but more precise compared to <see cref="Vector2f.Lerp(ref Vector2f, ref Vector2f, float, out Vector2f)"/>.
		/// See remarks section of <see cref="Maths.LerpPrecise"/> on MathHelper for more info.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
		/// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
		public static void LerpPrecise(ref Vector2f value1, ref Vector2f value2, float amount, out Vector2f result)
		{
			result.X = Maths.LerpPrecise(value1.X, value2.X, amount);
			result.Y = Maths.LerpPrecise(value1.Y, value2.Y, amount);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a maximal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The <see cref="Vector2f"/> with maximal values from the two vectors.</returns>
		public static Vector2f Max(Vector2f value1, Vector2f value2)
		{
			return new Vector2f(value1.X > value2.X ? value1.X : value2.X,
							   value1.Y > value2.Y ? value1.Y : value2.Y);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a maximal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The <see cref="Vector2f"/> with maximal values from the two vectors as an output parameter.</param>
		public static void Max(ref Vector2f value1, ref Vector2f value2, out Vector2f result)
		{
			result.X = value1.X > value2.X ? value1.X : value2.X;
			result.Y = value1.Y > value2.Y ? value1.Y : value2.Y;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a minimal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <returns>The <see cref="Vector2f"/> with minimal values from the two vectors.</returns>
		public static Vector2f Min(Vector2f value1, Vector2f value2)
		{
			return new Vector2f(value1.X < value2.X ? value1.X : value2.X,
							   value1.Y < value2.Y ? value1.Y : value2.Y);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a minimal values from the two vectors.
		/// </summary>
		/// <param name="value1">The first vector.</param>
		/// <param name="value2">The second vector.</param>
		/// <param name="result">The <see cref="Vector2f"/> with minimal values from the two vectors as an output parameter.</param>
		public static void Min(ref Vector2f value1, ref Vector2f value2, out Vector2f result)
		{
			result.X = value1.X < value2.X ? value1.X : value2.X;
			result.Y = value1.Y < value2.Y ? value1.Y : value2.Y;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a multiplication of two vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="value2">Source <see cref="Vector2f"/>.</param>
		/// <returns>The result of the vector multiplication.</returns>
		public static Vector2f Multiply(Vector2f value1, Vector2f value2)
		{
			value1.X *= value2.X;
			value1.Y *= value2.Y;
			return value1;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a multiplication of two vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="value2">Source <see cref="Vector2f"/>.</param>
		/// <param name="result">The result of the vector multiplication as an output parameter.</param>
		public static void Multiply(ref Vector2f value1, ref Vector2f value2, out Vector2f result)
		{
			result.X = value1.X * value2.X;
			result.Y = value1.Y * value2.Y;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a multiplication of <see cref="Vector2f"/> and a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <returns>The result of the vector multiplication with a scalar.</returns>
		public static Vector2f Multiply(Vector2f value1, float scaleFactor)
		{
			value1.X *= scaleFactor;
			value1.Y *= scaleFactor;
			return value1;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a multiplication of <see cref="Vector2f"/> and a scalar.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="scaleFactor">Scalar value.</param>
		/// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
		public static void Multiply(ref Vector2f value1, float scaleFactor, out Vector2f result)
		{
			result.X = value1.X * scaleFactor;
			result.Y = value1.Y * scaleFactor;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains the specified vector inversion.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2f"/>.</param>
		/// <returns>The result of the vector inversion.</returns>
		public static Vector2f Negate(Vector2f value)
		{
			value.X = -value.X;
			value.Y = -value.Y;
			return value;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains the specified vector inversion.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2f"/>.</param>
		/// <param name="result">The result of the vector inversion as an output parameter.</param>
		public static void Negate(ref Vector2f value, out Vector2f result)
		{
			result.X = -value.X;
			result.Y = -value.Y;
		}

		/// <summary>
		/// Turns this <see cref="Vector2f"/> to a unit vector with the same direction.
		/// </summary>
		public void Normalize()
		{
			float val = 1.0f / (float)Math.Sqrt((X * X) + (Y * Y));
			X *= val;
			Y *= val;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a normalized values from another vector.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2f"/>.</param>
		/// <returns>Unit vector.</returns>
		public static Vector2f Normalize(Vector2f value)
		{
			float val = 1.0f / (float)Math.Sqrt((value.X * value.X) + (value.Y * value.Y));
			value.X *= val;
			value.Y *= val;
			return value;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains a normalized values from another vector.
		/// </summary>
		/// <param name="value">Source <see cref="Vector2f"/>.</param>
		/// <param name="result">Unit vector as an output parameter.</param>
		public static void Normalize(ref Vector2f value, out Vector2f result)
		{
			float val = 1.0f / (float)Math.Sqrt((value.X * value.X) + (value.Y * value.Y));
			result.X = value.X * val;
			result.Y = value.Y * val;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains reflect vector of the given vector and normal.
		/// </summary>
		/// <param name="vector">Source <see cref="Vector2f"/>.</param>
		/// <param name="normal">Reflection normal.</param>
		/// <returns>Reflected vector.</returns>
		public static Vector2f Reflect(Vector2f vector, Vector2f normal)
		{
			Vector2f result;
			float val = 2.0f * ((vector.X * normal.X) + (vector.Y * normal.Y));
			result.X = vector.X - (normal.X * val);
			result.Y = vector.Y - (normal.Y * val);
			return result;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains reflect vector of the given vector and normal.
		/// </summary>
		/// <param name="vector">Source <see cref="Vector2f"/>.</param>
		/// <param name="normal">Reflection normal.</param>
		/// <param name="result">Reflected vector as an output parameter.</param>
		public static void Reflect(ref Vector2f vector, ref Vector2f normal, out Vector2f result)
		{
			float val = 2.0f * ((vector.X * normal.X) + (vector.Y * normal.Y));
			result.X = vector.X - (normal.X * val);
			result.Y = vector.Y - (normal.Y * val);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains cubic interpolation of the specified vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="value2">Source <see cref="Vector2f"/>.</param>
		/// <param name="amount">Weighting value.</param>
		/// <returns>Cubic interpolation of the specified vectors.</returns>
		public static Vector2f SmoothStep(Vector2f value1, Vector2f value2, float amount)
		{
			return new Vector2f(
				Maths.SmoothStep(value1.X, value2.X, amount),
				Maths.SmoothStep(value1.Y, value2.Y, amount));
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains cubic interpolation of the specified vectors.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="value2">Source <see cref="Vector2f"/>.</param>
		/// <param name="amount">Weighting value.</param>
		/// <param name="result">Cubic interpolation of the specified vectors as an output parameter.</param>
		public static void SmoothStep(ref Vector2f value1, ref Vector2f value2, float amount, out Vector2f result)
		{
			result.X = Maths.SmoothStep(value1.X, value2.X, amount);
			result.Y = Maths.SmoothStep(value1.Y, value2.Y, amount);
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains subtraction of on <see cref="Vector2f"/> from a another.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="value2">Source <see cref="Vector2f"/>.</param>
		/// <returns>The result of the vector subtraction.</returns>
		public static Vector2f Subtract(Vector2f value1, Vector2f value2)
		{
			value1.X -= value2.X;
			value1.Y -= value2.Y;
			return value1;
		}

		/// <summary>
		/// Creates a new <see cref="Vector2f"/> that contains subtraction of on <see cref="Vector2f"/> from a another.
		/// </summary>
		/// <param name="value1">Source <see cref="Vector2f"/>.</param>
		/// <param name="value2">Source <see cref="Vector2f"/>.</param>
		/// <param name="result">The result of the vector subtraction as an output parameter.</param>
		public static void Subtract(ref Vector2f value1, ref Vector2f value2, out Vector2f result)
		{
			result.X = value1.X - value2.X;
			result.Y = value1.Y - value2.Y;
		}

		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="Vector2f"/> in the format:
		/// {X:[<see cref="X"/>] Y:[<see cref="Y"/>]}
		/// </summary>
		/// <returns>A <see cref="String"/> representation of this <see cref="Vector2f"/>.</returns>
		public override string ToString()
		{
			return "{X:" + X + " Y:" + Y + "}";
		}

		#endregion
	}
}