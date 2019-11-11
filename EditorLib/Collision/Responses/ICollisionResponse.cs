using AuxLib;

namespace AuxLib.CollisionDetection.Responses
{
    public delegate CollisionResponses OnCollisionDelegate(ICollision collision);
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

