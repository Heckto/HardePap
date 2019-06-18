﻿using Game1.CollisionDetection.Base;

namespace Game1.CollisionDetection.Responses
{
	
	/// <summary>
	/// The result of a collision reaction onto a box position.
	/// </summary>
	public interface ICollisionResponse
	{
		/// <summary>
		/// Gets the new destination of the box after the collision.
		/// </summary>
		/// <value>The destination.</value>
		RectangleF Destination { get; }
	}
}
